using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using SSharp.Backend;
using SSharp.Compiler;

namespace SSharp.Api.Services;

/// <summary>gRPC implementation of the stateless SSharp eval service.</summary>
public class EvalGrpcService : EvalService.EvalServiceBase
{
    private readonly EvalBackend _backend = new();

    public override Task<EvalResponse> Eval(EvalRequest request, ServerCallContext context)
    {
        var result = EvalCore(request.Code);
        return Task.FromResult(result);
    }

    public static EvalResponse EvalCore(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return new EvalResponse { Success = false, Errors = { "Code cannot be empty." } };
        }

        // ── Lex ──────────────────────────────────────────────────────────────
        var lexer  = new Lexer(code);
        var tokens = lexer.ScanTokens();

        var lexErrors = tokens.Where(t => t.Type == TokenType.Error).ToList();
        if (lexErrors.Any())
        {
            var resp = new EvalResponse { Success = false };
            foreach (var e in lexErrors)
                resp.Errors.Add($"[{e.Line}:{e.Column}] {e.Lexeme}");
            return resp;
        }

        // ── Parse ─────────────────────────────────────────────────────────────
        var parser = new Parser(tokens);
        var ast    = parser.ParseProgram();

        if (parser.Errors.Any())
        {
            var resp = new EvalResponse { Success = false };
            foreach (var e in parser.Errors) resp.Errors.Add(e);
            return resp;
        }

        // ── Type Check ────────────────────────────────────────────────────────
        var typeChecker = new TypeChecker();
        typeChecker.Check(ast);

        if (typeChecker.Errors.Any())
        {
            var resp = new EvalResponse { Success = false };
            foreach (var e in typeChecker.Errors) resp.Errors.Add(e);
            return resp;
        }

        // ── Determine Type Info ────────────────────────────────────────────────
        string? typeInfo = null;
        var lastDecl = ast.Decls.LastOrDefault();
        if (lastDecl is ExprDecl exprDecl)
        {
            if (typeChecker.ResolvedTypes.TryGetValue(exprDecl.Expression, out var t))
            {
                typeInfo = t.ToString();
            }
        }
        else if (lastDecl is ValDecl valDecl)
        {
            if (typeChecker.ResolvedTypes.TryGetValue(valDecl.Value, out var t))
            {
                typeInfo = t.ToString();
            }
            else if (valDecl.Type != null)
            {
                typeInfo = valDecl.Type.ToString();
            }
        }
        else if (lastDecl is FunDecl funDecl)
        {
            var paramTypes = funDecl.Params.Select(p => p.Type.ToString());
            string retType = funDecl.ReturnType?.ToString() ?? "Unit";
            typeInfo = $"({string.Join(", ", paramTypes)}) => {retType}";
        }

        // ── Code Gen ──────────────────────────────────────────────────────────
        var codeGen    = new CodeGenerator(typeChecker.ResolvedTypes);
        string genCode = codeGen.Generate(ast);

        // ── Eval ──────────────────────────────────────────────────────────────
        var backend    = new EvalBackend();
        var evalResult = backend.Eval(genCode, typeInfo: typeInfo);

        var response = new EvalResponse
        {
            Success   = evalResult.Success,
            Output    = evalResult.Output,
            ElapsedMs = evalResult.ElapsedMs,
            TypeInfo  = evalResult.TypeInfo ?? string.Empty,
        };
        foreach (var e in evalResult.Errors) response.Errors.Add(e);
        return response;
    }
}
