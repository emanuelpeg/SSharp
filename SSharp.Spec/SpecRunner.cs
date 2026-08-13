using System;
using System.Collections.Generic;
using System.Linq;
using SSharp.Backend;
using SSharp.Compiler;

namespace SSharp.Spec;

public record ExecutionResult(bool Success, string Output, IReadOnlyList<string> Errors);

public class SpecRunner
{
    private readonly EvalBackend _evalBackend = new();

    public ExecutionResult RunSSharp(string ssharpSource)
    {
        var lexer = new Lexer(ssharpSource);
        var tokens = lexer.ScanTokens();

        var lexerErrors = tokens.Where(t => t.Type == TokenType.Error).ToList();
        if (lexerErrors.Any())
        {
            return new ExecutionResult(false, string.Empty, lexerErrors.Select(e => $"Lexer error at [{e.Line}:{e.Column}]: {e.Value}").ToList());
        }

        var parser = new Parser(tokens);
        var ast = parser.ParseProgram();
        if (parser.Errors.Any())
        {
            return new ExecutionResult(false, string.Empty, parser.Errors);
        }

        var typeChecker = new TypeChecker();
        typeChecker.Check(ast);
        if (typeChecker.Errors.Any())
        {
            return new ExecutionResult(false, string.Empty, typeChecker.Errors);
        }

        var codeGenerator = new CodeGenerator(typeChecker.ResolvedTypes);
        string csharpCode = codeGenerator.Generate(ast);

        var evalResult = _evalBackend.Eval(csharpCode);
        return new ExecutionResult(evalResult.Success, NormalizeOutput(evalResult.Output), evalResult.Errors);
    }

    public ExecutionResult RunCSharp(string csharpSource)
    {
        var evalResult = _evalBackend.Eval(csharpSource);
        return new ExecutionResult(evalResult.Success, NormalizeOutput(evalResult.Output), evalResult.Errors);
    }

    public static string NormalizeOutput(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return string.Empty;
        return raw.Replace("\r\n", "\n").TrimEnd();
    }
}
