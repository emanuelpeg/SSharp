using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SSharp.Backend;
using SSharp.Compiler;

namespace SSharp.Repl;

/// <summary>
/// Stateful REPL session for SSharp.
///
/// <para>
/// Each call to <see cref="Submit"/> appends the new snippet to the accumulated context
/// and runs the full compiler pipeline (Lex → Parse → TypeCheck → CodeGen → Eval).
/// If the snippet produces any error at any phase, the context is <em>not</em> updated
/// and the errors are returned to the caller.
/// </para>
///
/// <para>
/// The session tracks a monotonically incrementing <c>resN</c> counter for anonymous
/// top-level expressions so they can be displayed as <c>res0: Int = 42</c>.
/// </para>
/// </summary>
public class ReplSession
{
    private readonly EvalBackend _evalBackend = new();

    /// <summary>Accumulated source lines that compiled successfully.</summary>
    private readonly StringBuilder _context = new();

    /// <summary>Counter for anonymous result bindings (res0, res1, …).</summary>
    private int _resCounter;

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Submit a new code snippet to the session.</summary>
    public ReplResult Submit(string snippet)
    {
        if (string.IsNullOrWhiteSpace(snippet))
            return new ReplResult(true, string.Empty, Array.Empty<string>(), 0);

        // ── Detect what the snippet ends with ─────────────────────────────────
        // We need to know whether the last meaningful declaration is a ValDecl,
        // a FunDecl, or a bare ExprDecl so we can wrap it for display.
        string trimmed = snippet.Trim();

        // Build the candidate full source = accumulated context + new snippet
        string candidateSource = BuildCandidateSource(trimmed, out string? wrappedSnippet,
                                                       out string? bindingName, out bool isExpr);

        // ── Lex ───────────────────────────────────────────────────────────────
        var lexer  = new Lexer(candidateSource);
        var tokens = lexer.ScanTokens();

        var lexErrors = tokens.Where(t => t.Type == TokenType.Error).ToList();
        if (lexErrors.Any())
        {
            var errs = lexErrors.Select(e => $"[{e.Line}:{e.Column}] {e.Lexeme}").ToList();
            return new ReplResult(false, string.Empty, errs, 0);
        }

        // ── Parse ─────────────────────────────────────────────────────────────
        var parser = new Parser(tokens);
        var ast    = parser.ParseProgram();

        if (parser.Errors.Any())
            return new ReplResult(false, string.Empty, parser.Errors, 0);

        // ── Type Check ────────────────────────────────────────────────────────
        var typeChecker = new TypeChecker();
        typeChecker.Check(ast);

        if (typeChecker.Errors.Any())
            return new ReplResult(false, string.Empty, typeChecker.Errors, 0);

        // ── Determine type info for the last declaration ───────────────────────
        string? typeInfo = ExtractTypeInfo(ast, typeChecker, isExpr, bindingName);

        // ── Code Generation ────────────────────────────────────────────────────
        var codeGen = new CodeGenerator(typeChecker.ResolvedTypes);
        string generatedCs = codeGen.Generate(ast);

        // ── Eval (in memory) ──────────────────────────────────────────────────
        var evalResult = _evalBackend.Eval(generatedCs, typeInfo: typeInfo);

        if (!evalResult.Success)
            return new ReplResult(false, evalResult.Output, evalResult.Errors, evalResult.ElapsedMs, typeInfo, bindingName);

        // ── Commit the snippet to context ─────────────────────────────────────
        if (_context.Length > 0) _context.AppendLine();
        _context.Append(trimmed);

        // Bump res counter if this was an anonymous expression
        if (isExpr && bindingName != null && bindingName.StartsWith("res"))
            _resCounter++;

        return new ReplResult(true, evalResult.Output, evalResult.Errors, evalResult.ElapsedMs, typeInfo, bindingName);
    }

    /// <summary>Reset the session context (like `:reset` in a REPL).</summary>
    public void Reset()
    {
        _context.Clear();
        _resCounter = 0;
    }

    /// <summary>The accumulated source code committed so far.</summary>
    public string Context => _context.ToString();

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Builds the full candidate source by prepending the accumulated context.
    /// Also figures out whether the snippet ends in a bare expression and, if so,
    /// wraps it in a synthetic <c>val resN = …</c> so we can display the type.
    /// </summary>
    private string BuildCandidateSource(
        string snippet,
        out string? wrappedSnippet,
        out string? bindingName,
        out bool isExpr)
    {
        wrappedSnippet = null;
        bindingName    = null;
        isExpr         = false;

        // Quick-parse the snippet alone to figure out what the last declaration is.
        // We do a lightweight parse just for classification — errors are ignored here.
        var snippetLexer  = new Lexer(snippet);
        var snippetTokens = snippetLexer.ScanTokens();
        var snippetParser = new Parser(snippetTokens);
        var snippetAst    = snippetParser.ParseProgram();

        var lastDecl = snippetAst.Decls.LastOrDefault();

        if (lastDecl is ValDecl val)
        {
            bindingName = val.Name;
            isExpr      = false;
        }
        else if (lastDecl is FunDecl fun)
        {
            bindingName = fun.Name;
            isExpr      = false;
        }
        else if (lastDecl is ExprDecl)
        {
            // Bare expression: wrap as `val resN = <expr>`
            string resName  = $"res{_resCounter}";
            bindingName     = resName;
            isExpr          = true;
            wrappedSnippet  = snippet;
            // Replace the bare ExprDecl with a val binding so the type checker can resolve it
            snippet = $"val {resName} = {snippet}";
        }

        // Combine context + new snippet
        string combined = _context.Length > 0
            ? _context + Environment.NewLine + snippet
            : snippet;

        return combined;
    }

    /// <summary>
    /// Extracts the human-readable SSharp type of the last interesting declaration
    /// from the type-checked AST.
    /// </summary>
    private static string? ExtractTypeInfo(
        Program ast,
        TypeChecker typeChecker,
        bool isExpr,
        string? bindingName)
    {
        if (bindingName == null) return null;

        // Find the last val or expr decl that matches the binding name
        foreach (var decl in ast.Decls.AsEnumerable().Reverse())
        {
            if (decl is ValDecl val && val.Name == bindingName)
            {
                if (typeChecker.ResolvedTypes.TryGetValue(val.Value, out var t))
                    return t.ToString();
                if (val.Type != null)
                    return val.Type.ToString();
                return null;
            }

            if (decl is FunDecl fun && fun.Name == bindingName)
            {
                // Build a function type string: (A, B) => C
                var paramTypes = fun.Params.Select(p => p.Type.ToString());
                string retType = fun.ReturnType?.ToString() ?? "?";
                return $"({string.Join(", ", paramTypes)}) => {retType}";
            }
        }

        return null;
    }
}
