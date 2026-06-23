using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSharp.Compiler;

public class CodeGenerator
{
    private readonly Dictionary<Expr, SSharpType> _resolvedTypes;
    private readonly HashSet<string> _caseObjects = new();
    private readonly Dictionary<string, ClassDecl> _classDecls = new();
    private readonly Stack<HashSet<string>> _lazyParamsStack = new();

    private bool IsLazyParam(string name)
    {
        foreach (var set in _lazyParamsStack)
        {
            if (set.Contains(name)) return true;
        }
        return false;
    }

    public CodeGenerator(Dictionary<Expr, SSharpType> resolvedTypes)
    {
        _resolvedTypes = resolvedTypes;
    }

    public string Generate(Program program)
    {
        // First pass: identify case objects and record all class declarations
        foreach (var decl in program.Decls)
        {
            if (decl is ClassDecl cls)
            {
                _classDecls[cls.Name] = cls;
                if (cls.IsCase && cls.ConstructorParams.Count == 0)
                {
                    _caseObjects.Add(cls.Name);
                }
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using SSharp.Runtime;");
        sb.AppendLine("using static SSharp.Runtime.Predef;");
        sb.AppendLine();
        sb.AppendLine("namespace SSharp.Generated;");
        sb.AppendLine();

        // 1. Generate traits and classes
        foreach (var decl in program.Decls)
        {
            if (decl is TraitDecl or ClassDecl)
            {
                sb.AppendLine(GenerateDecl(decl, indent: 0));
                sb.AppendLine();
            }
        }

        // 2. Generate Program class for top-level functions, values, and entry point
        sb.AppendLine("public static class Program");
        sb.AppendLine("{");

        // Generate factory methods for case classes to allow calling them without "new"
        foreach (var decl in program.Decls)
        {
            if (decl is ClassDecl cls && cls.IsCase && cls.ConstructorParams.Count > 0)
            {
                string tparams = cls.TypeParams.Count > 0 ? $"<{string.Join(", ", cls.TypeParams)}>" : "";
                string typeName = $"{cls.Name}{tparams}";
                string parameters = string.Join(", ", cls.ConstructorParams.Select(p => $"{MapTypeNode(p.Type)} {p.Name}"));
                string arguments = string.Join(", ", cls.ConstructorParams.Select(p => p.Name));
                sb.AppendLine($"    public static {typeName} {cls.Name}{tparams}({parameters}) => new {typeName}({arguments});");
            }
        }

        if (program.Decls.Any(d => d is ClassDecl cls && cls.IsCase && cls.ConstructorParams.Count > 0))
        {
            sb.AppendLine();
        }

        // Generate top-level values and functions
        foreach (var decl in program.Decls)
        {
            if (decl is ValDecl or FunDecl)
            {
                sb.AppendLine(GenerateDecl(decl, indent: 4));
            }
        }

        // Generate entry point Main
        sb.AppendLine("    public static void Main(string[] args)");
        sb.AppendLine("    {");
        
        // Execute all top-level expressions
        foreach (var decl in program.Decls)
        {
            if (decl is ExprDecl exprDecl)
            {
                sb.AppendLine($"        {GenerateExpr(exprDecl.Expression)};");
            }
        }

        // Check if there is a top-level def main
        bool hasMain = program.Decls.Any(d => d is FunDecl f && f.Name == "main");
        if (hasMain)
        {
            sb.AppendLine("        main();");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private string GenerateDecl(Decl decl, int indent)
    {
        string spaces = new string(' ', indent);
        switch (decl)
        {
            case ImportDecl imp:
                return $"{spaces}using {imp.Path.Trim('\"')};";

            case TraitDecl trait:
                {
                    string tparams = trait.TypeParams.Count > 0 ? $"<{string.Join(", ", trait.TypeParams)}>" : "";
                    return $"{spaces}public abstract record {trait.Name}{tparams};";
                }

            case ClassDecl cls:
                {
                    string tparams = cls.TypeParams.Count > 0 ? $"<{string.Join(", ", cls.TypeParams)}>" : "";
                    string extends = cls.ExtendsType != null ? $" : {MapTypeNode(cls.ExtendsType)}" : "";
                    
                    if (cls.IsCase && cls.ConstructorParams.Count == 0)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine($"{spaces}public record {cls.Name}{extends}");
                        sb.AppendLine($"{spaces}{{");
                        sb.AppendLine($"{spaces}    private {cls.Name}() {{}}");
                        sb.AppendLine($"{spaces}    public static {cls.Name} Instance {{ get; }} = new {cls.Name}();");
                        sb.Append($"{spaces}}}");
                        return sb.ToString();
                    }
                    else
                    {
                        string parameters = string.Join(", ", cls.ConstructorParams.Select(p => $"{MapTypeNode(p.Type)} {EscapeIdentifier(p.Name)}"));
                        return $"{spaces}public record {cls.Name}{tparams}({parameters}){extends};";
                    }
                }

            case ValDecl val:
                {
                    string typeStr;
                    if (val.Type != null)
                    {
                        typeStr = MapTypeNode(val.Type);
                    }
                    else if (_resolvedTypes.TryGetValue(val.Value, out var t))
                    {
                        typeStr = MapType(t);
                    }
                    else
                    {
                        typeStr = "object";
                    }

                    if (indent == 4)
                    {
                        return $"{spaces}public static readonly {typeStr} {EscapeIdentifier(val.Name)} = {GenerateExpr(val.Value)};";
                    }
                    else
                    {
                        return $"{spaces}var {val.Name} = {GenerateExpr(val.Value)};";
                    }
                }

            case FunDecl fun:
                {
                    string tparams = fun.TypeParams.Count > 0 ? $"<{string.Join(", ", fun.TypeParams)}>" : "";
                    string retType;
                    if (fun.ReturnType != null)
                    {
                        retType = MapTypeNode(fun.ReturnType);
                    }
                    else if (_resolvedTypes.TryGetValue(fun.Body, out var t))
                    {
                        retType = MapType(t);
                    }
                    else
                    {
                        retType = "object";
                    }

                    string parameters = string.Join(", ", fun.Params.Select(p => $"{MapTypeNode(p.Type)} {EscapeIdentifier(p.Name)}"));
                    
                    var lazyInThisFun = new HashSet<string>(
                        fun.Params.Where(p => p.Type.IsLazy).Select(p => p.Name)
                    );
                    _lazyParamsStack.Push(lazyInThisFun);

                    string generatedBody;
                    if (fun.IsTailRec)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine($"{spaces}public static {retType} {EscapeIdentifier(fun.Name)}{tparams}({parameters})");
                        sb.AppendLine($"{spaces}{{");
                        sb.AppendLine($"{spaces}    while (true)");
                        sb.AppendLine($"{spaces}    {{");
                        sb.Append(GenerateTailRecBody(fun.Body, fun.Name, fun.Params, retType, indent + 8));
                        sb.AppendLine();
                        sb.AppendLine($"{spaces}    }}");
                        sb.Append($"{spaces}}}");
                        generatedBody = sb.ToString();
                    }
                    else if (fun.Body is BlockExpr block)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine($"{spaces}public static {retType} {EscapeIdentifier(fun.Name)}{tparams}({parameters})");
                        sb.AppendLine($"{spaces}{{");
                        sb.Append(GenerateBlockBody(block, indent + 4, retType));
                        sb.AppendLine();
                        sb.Append($"{spaces}}}");
                        generatedBody = sb.ToString();
                    }
                    else
                    {
                        generatedBody = $"{spaces}public static {retType} {EscapeIdentifier(fun.Name)}{tparams}({parameters}) => {GenerateExpr(fun.Body)};";
                    }

                    _lazyParamsStack.Pop();
                    return generatedBody;
                }

            default:
                return "";
        }
    }

    private string GenerateLocalDecl(Decl decl, int indent)
    {
        string spaces = new string(' ', indent);
        switch (decl)
        {
            case ValDecl val:
                return $"{spaces}var {EscapeIdentifier(val.Name)} = {GenerateExpr(val.Value)};";

            case FunDecl fun:
                {
                    string retType;
                    if (fun.ReturnType != null)
                    {
                        retType = MapTypeNode(fun.ReturnType);
                    }
                    else if (_resolvedTypes.TryGetValue(fun.Body, out var t))
                    {
                        retType = MapType(t);
                    }
                    else
                    {
                        retType = "object";
                    }

                    string parameters = string.Join(", ", fun.Params.Select(p => $"{MapTypeNode(p.Type)} {p.Name}"));
                    
                    var lazyInThisFun = new HashSet<string>(
                        fun.Params.Where(p => p.Type.IsLazy).Select(p => p.Name)
                    );
                    _lazyParamsStack.Push(lazyInThisFun);

                    string generatedBody;
                    if (fun.IsTailRec)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine($"{spaces}{retType} {fun.Name}({parameters})");
                        sb.AppendLine($"{spaces}{{");
                        sb.AppendLine($"{spaces}    while (true)");
                        sb.AppendLine($"{spaces}    {{");
                        sb.Append(GenerateTailRecBody(fun.Body, fun.Name, fun.Params, retType, indent + 8));
                        sb.AppendLine();
                        sb.AppendLine($"{spaces}    }}");
                        sb.Append($"{spaces}}}");
                        generatedBody = sb.ToString();
                    }
                    else if (fun.Body is BlockExpr block)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine($"{spaces}{retType} {fun.Name}({parameters})");
                        sb.AppendLine($"{spaces}{{");
                        sb.Append(GenerateBlockBody(block, indent + 4, retType));
                        sb.AppendLine();
                        sb.Append($"{spaces}}}");
                        generatedBody = sb.ToString();
                    }
                    else
                    {
                        generatedBody = $"{spaces}{retType} {fun.Name}({parameters}) => {GenerateExpr(fun.Body)};";
                    }

                    _lazyParamsStack.Pop();
                    return generatedBody;
                }
            default:
                return "";
        }
    }

    private string GenerateBlockBody(BlockExpr block, int indent, string retType)
    {
        string spaces = new string(' ', indent);
        var sb = new StringBuilder();
        
        foreach (var elem in block.Elements)
        {
            if (elem is Decl d)
            {
                sb.AppendLine(GenerateLocalDecl(d, indent));
            }
            else if (elem is Expr e)
            {
                sb.AppendLine($"{spaces}{GenerateExpr(e)};");
            }
        }

        if (block.FinalExpr != null)
        {
            sb.Append($"{spaces}return {GenerateExpr(block.FinalExpr)};");
        }
        else
        {
            sb.Append($"{spaces}return SSharp.Runtime.Unit.Instance;");
        }

        return sb.ToString();
    }

    private string GenerateBlockExpr(BlockExpr block)
    {
        string retType;
        if (_resolvedTypes.TryGetValue(block, out var t))
        {
            retType = MapType(t);
        }
        else
        {
            retType = "SSharp.Runtime.Unit";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"new System.Func<{retType}>(() =>");
        sb.AppendLine("{");
        sb.Append(GenerateBlockBody(block, 4, retType));
        sb.AppendLine();
        sb.Append("})()");
        return sb.ToString();
    }

    private string GenerateExpr(Expr expr)
    {
        switch (expr)
        {
            case LiteralExpr lit:
                if (lit.Type == TokenType.StringLiteral)
                {
                    return EscapeString(lit.Value?.ToString() ?? "");
                }
                if (lit.Type == TokenType.DoubleLiteral)
                {
                    return lit.Value?.ToString() + "d";
                }
                if (lit.Type == TokenType.True) return "true";
                if (lit.Type == TokenType.False) return "false";
                return lit.Value?.ToString() ?? "null";

            case UnitExpr:
                return "SSharp.Runtime.Unit.Instance";

            case IdentifierExpr id:
                if (IsLazyParam(id.Name))
                {
                    return $"{EscapeIdentifier(id.Name)}()";
                }
                if (id.Name == "Nil")
                {
                    if (_resolvedTypes.TryGetValue(id, out var t) && t is GenericType gt && gt.TypeArgs.Count > 0)
                    {
                        return $"new SSharp.Runtime.Nil<{MapType(gt.TypeArgs[0])}>()";
                    }
                    return "new SSharp.Runtime.Nil<object>()";
                }
                if (id.Name == "None")
                {
                    if (_resolvedTypes.TryGetValue(id, out var t) && t is GenericType gt && gt.TypeArgs.Count > 0)
                    {
                        return $"new SSharp.Runtime.None<{MapType(gt.TypeArgs[0])}>()";
                    }
                    return "new SSharp.Runtime.None<object>()";
                }
                if (_caseObjects.Contains(id.Name))
                {
                    return $"{id.Name}.Instance";
                }
                return EscapeIdentifier(id.Name);

            case BinaryExpr bin:
                return $"({GenerateExpr(bin.Lhs)} {bin.Op.Lexeme} {GenerateExpr(bin.Rhs)})";

            case UnaryExpr unary:
                return $"({unary.Op.Lexeme}{GenerateExpr(unary.Expr)})";

            case BlockExpr block:
                return GenerateBlockExpr(block);

            case IfExpr condExpr:
                return $"({GenerateExpr(condExpr.Condition)} ? {GenerateExpr(condExpr.ThenBranch)} : {GenerateExpr(condExpr.ElseBranch)})";

            case CallExpr call:
                {
                    string calleeStr = GenerateExpr(call.Callee);

                    if (_resolvedTypes.TryGetValue(call.Callee, out var calleeType) && calleeType is FunctionType funType)
                    {
                        int expectedCount = funType.ParamTypes.Count;
                        int actualCount = call.Arguments.Count;

                        var argsList = new List<string>();
                        for (int i = 0; i < actualCount; i++)
                        {
                            var arg = call.Arguments[i];
                            if (i < expectedCount && funType.ParamTypes[i] is ByNameType bt)
                            {
                                string underTypeStr = MapType(bt.UnderType);
                                argsList.Add($"new System.Func<{underTypeStr}>(() => {GenerateExpr(arg)})");
                            }
                            else
                            {
                                argsList.Add(GenerateExpr(arg));
                            }
                        }

                        if (actualCount < expectedCount)
                        {
                            var remainingTypes = funType.ParamTypes.GetRange(actualCount, expectedCount - actualCount);
                            var remainingNames = Enumerable.Range(0, remainingTypes.Count).Select(i => $"_p{i}").ToList();

                            string delegateType = remainingTypes.Count == 0 
                                ? $"System.Func<{MapType(funType.ReturnType)}>"
                                : $"System.Func<{string.Join(", ", remainingTypes.Select(MapType))}, {MapType(funType.ReturnType)}>";

                            var allArgs = new List<string>();
                            allArgs.AddRange(argsList);
                            allArgs.AddRange(remainingNames);

                            string typeArgsStr = call.TypeArgs.Count > 0 
                                ? $"<{string.Join(", ", call.TypeArgs.Select(MapTypeNode))}>"
                                : "";

                            return $"new {delegateType}(({string.Join(", ", remainingNames)}) => {calleeStr}{typeArgsStr}({string.Join(", ", allArgs)}))";
                        }

                        string typeArgsStr2 = call.TypeArgs.Count > 0 
                            ? $"<{string.Join(", ", call.TypeArgs.Select(MapTypeNode))}>"
                            : "";
                        return $"{calleeStr}{typeArgsStr2}({string.Join(", ", argsList)})";
                    }

                    string typeArgsStr3 = call.TypeArgs.Count > 0 
                        ? $"<{string.Join(", ", call.TypeArgs.Select(MapTypeNode))}>"
                        : "";
                    string argsStr = string.Join(", ", call.Arguments.Select(GenerateExpr));
                    return $"{calleeStr}{typeArgsStr3}({argsStr})";
                }

            case LambdaExpr lambda:
                {
                    string retTypeStr = "object";
                    var paramTypes = new List<string>();
                    if (_resolvedTypes.TryGetValue(lambda, out var t) && t is FunctionType ft)
                    {
                        retTypeStr = MapType(ft.ReturnType);
                        paramTypes = ft.ParamTypes.Select(MapType).ToList();
                    }
                    else
                    {
                        paramTypes = lambda.Params.Select(p => MapTypeNode(p.Type)).ToList();
                    }

                    string delegateType = paramTypes.Count == 0 
                        ? $"System.Func<{retTypeStr}>"
                        : $"System.Func<{string.Join(", ", paramTypes)}, {retTypeStr}>";

                    string paramNames = string.Join(", ", lambda.Params.Select(p => p.Name));
                    
                    if (lambda.Body is BlockExpr block)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine($"new {delegateType}(({paramNames}) => {{");
                        sb.Append(GenerateBlockBody(block, 4, retTypeStr));
                        sb.AppendLine();
                        sb.Append("})");
                        return sb.ToString();
                    }
                    else
                    {
                        return $"new {delegateType}(({paramNames}) => {GenerateExpr(lambda.Body)})";
                    }
                }

            case MatchExpr match:
                return GenerateMatchExpr(match);

            default:
                throw new Exception($"Unknown expression type: {expr.GetType().Name}");
        }
    }

    private string GenerateMatchExpr(MatchExpr match)
    {
        SSharpType matchedType;
        if (!_resolvedTypes.TryGetValue(match.Expression, out matchedType!))
        {
            matchedType = SSharpType.Any;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"({GenerateExpr(match.Expression)}) switch");
        sb.AppendLine("{");
        
        foreach (var c in match.Cases)
        {
            string patternStr = GeneratePattern(c.Pattern, matchedType);
            sb.AppendLine($"    {patternStr} => {GenerateExpr(c.Body)},");
        }
        
        sb.AppendLine("    _ => throw new System.InvalidOperationException(\"Pattern match failed\")");
        sb.Append("}");
        return sb.ToString();
    }

    private string GeneratePattern(Pattern pattern, SSharpType matchedType)
    {
        switch (pattern)
        {
            case WildcardPattern:
                return "_";

            case LiteralPattern lit:
                if (lit.Type == TokenType.StringLiteral)
                {
                    return EscapeString(lit.Value?.ToString() ?? "");
                }
                if (lit.Type == TokenType.True) return "true";
                if (lit.Type == TokenType.False) return "false";
                return lit.Value?.ToString() ?? "null";

            case IdentifierPattern id:
                return $"var {id.Name}";

            case ConstructorPattern cons:
                {
                    if (_classDecls.TryGetValue(cons.Name, out var clsDecl))
                    {
                        var typeMap = new Dictionary<string, SSharpType>();
                        if (matchedType is GenericType gt && clsDecl.ExtendsType != null && clsDecl.ExtendsType.TypeArgs.Count > 0)
                        {
                            for (int i = 0; i < Math.Min(clsDecl.ExtendsType.TypeArgs.Count, gt.TypeArgs.Count); i++)
                            {
                                var extendsArg = clsDecl.ExtendsType.TypeArgs[i];
                                typeMap[extendsArg.Name] = gt.TypeArgs[i];
                            }
                        }

                        string typeName = clsDecl.Name;
                        if (clsDecl.TypeParams.Count > 0)
                        {
                            var typeArgs = new List<string>();
                            foreach (var tp in clsDecl.TypeParams)
                            {
                                if (typeMap.TryGetValue(tp, out var t))
                                {
                                    typeArgs.Add(MapType(t));
                                }
                                else
                                {
                                    typeArgs.Add("object");
                                }
                            }
                            typeName += $"<{string.Join(", ", typeArgs)}>";
                        }

                        if (cons.SubPatterns.Count > 0)
                        {
                            var subPatternTypes = new List<SSharpType>();
                            for (int i = 0; i < clsDecl.ConstructorParams.Count; i++)
                            {
                                var paramDecl = clsDecl.ConstructorParams[i];
                                SSharpType resolvedParamType = ResolveType(paramDecl.Type);
                                resolvedParamType = ApplyTypeMap(resolvedParamType, typeMap);
                                subPatternTypes.Add(resolvedParamType);
                            }

                            var subPatternsStrs = new List<string>();
                            for (int i = 0; i < Math.Min(cons.SubPatterns.Count, subPatternTypes.Count); i++)
                            {
                                subPatternsStrs.Add(GeneratePattern(cons.SubPatterns[i], subPatternTypes[i]));
                            }
                            return $"{typeName}({string.Join(", ", subPatternsStrs)})";
                        }
                        else
                        {
                            if (clsDecl.ConstructorParams.Count == 0)
                            {
                                return typeName;
                            }
                            else
                            {
                                return $"{typeName}()";
                            }
                        }
                    }
                    else
                    {
                        // Fallback for runtime library classes e.g. Cons, Nil, Some, None
                        if (cons.Name is "Nil" or "None")
                        {
                            string typeArgStr = "object";
                            if (matchedType is GenericType gt && gt.TypeArgs.Count > 0)
                            {
                                typeArgStr = MapType(gt.TypeArgs[0]);
                            }
                            return $"SSharp.Runtime.{cons.Name}<{typeArgStr}>";
                        }
                        if (cons.Name is "Cons" or "Some")
                        {
                            string typeArgStr = "object";
                            if (matchedType is GenericType gt && gt.TypeArgs.Count > 0)
                            {
                                typeArgStr = MapType(gt.TypeArgs[0]);
                            }
                            
                            var subPatternsStrs = new List<string>();
                            if (cons.Name == "Cons")
                            {
                                var headType = matchedType is GenericType gt2 && gt2.TypeArgs.Count > 0 ? gt2.TypeArgs[0] : SSharpType.Any;
                                var tailType = matchedType;
                                if (cons.SubPatterns.Count > 0) subPatternsStrs.Add(GeneratePattern(cons.SubPatterns[0], headType));
                                if (cons.SubPatterns.Count > 1) subPatternsStrs.Add(GeneratePattern(cons.SubPatterns[1], tailType));
                            }
                            else if (cons.Name == "Some")
                            {
                                var valType = matchedType is GenericType gt2 && gt2.TypeArgs.Count > 0 ? gt2.TypeArgs[0] : SSharpType.Any;
                                if (cons.SubPatterns.Count > 0) subPatternsStrs.Add(GeneratePattern(cons.SubPatterns[0], valType));
                            }

                            return $"SSharp.Runtime.{cons.Name}<{typeArgStr}>({string.Join(", ", subPatternsStrs)})";
                        }

                        return cons.Name;
                    }
                }

            default:
                return "_";
        }
    }

    private SSharpType ApplyTypeMap(SSharpType type, Dictionary<string, SSharpType> typeMap)
    {
        return type switch
        {
            PrimitiveType pt => typeMap.TryGetValue(pt.Name, out var mapped) ? mapped : pt,
            GenericType gt => new GenericType(gt.Name, gt.TypeArgs.Select(t => ApplyTypeMap(t, typeMap)).ToList()),
            FunctionType ft => new FunctionType(ft.ParamTypes.Select(t => ApplyTypeMap(t, typeMap)).ToList(), ApplyTypeMap(ft.ReturnType, typeMap)),
            _ => type
        };
    }

    private SSharpType ResolveType(TypeNode node)
    {
        if (node.Name == "Int") return SSharpType.Int;
        if (node.Name == "Double") return SSharpType.Double;
        if (node.Name == "String") return SSharpType.String;
        if (node.Name == "Boolean") return SSharpType.Boolean;
        if (node.Name == "Unit") return SSharpType.Unit;
        if (node.Name == "Any") return SSharpType.Any;

        var args = new List<SSharpType>();
        foreach (var arg in node.TypeArgs)
        {
            args.Add(ResolveType(arg));
        }

        if (args.Count > 0)
        {
            return new GenericType(node.Name, args);
        }
        return new PrimitiveType(node.Name);
    }

    private string MapType(SSharpType type)
    {
        return type switch
        {
            PrimitiveType pt => pt.Name switch
            {
                "Int" => "int",
                "Double" => "double",
                "String" => "string",
                "Boolean" => "bool",
                "Unit" => "SSharp.Runtime.Unit",
                "Any" => "object",
                _ => pt.Name
            },
            GenericType gt => gt.Name switch
            {
                "List" => $"SSharp.Runtime.SSharpList<{string.Join(", ", gt.TypeArgs.Select(MapType))}>",
                "Option" => $"SSharp.Runtime.SSharpOption<{string.Join(", ", gt.TypeArgs.Select(MapType))}>",
                "Some" => $"SSharp.Runtime.Some<{string.Join(", ", gt.TypeArgs.Select(MapType))}>",
                "None" => $"SSharp.Runtime.None<{string.Join(", ", gt.TypeArgs.Select(MapType))}>",
                "Cons" => $"SSharp.Runtime.Cons<{string.Join(", ", gt.TypeArgs.Select(MapType))}>",
                "Nil" => $"SSharp.Runtime.Nil<{string.Join(", ", gt.TypeArgs.Select(MapType))}>",
                _ => $"{gt.Name}<{string.Join(", ", gt.TypeArgs.Select(MapType))}>"
            },
            FunctionType ft => ft.ParamTypes.Count == 0 
                ? $"System.Func<{MapType(ft.ReturnType)}>"
                : $"System.Func<{string.Join(", ", ft.ParamTypes.Select(MapType))}, {MapType(ft.ReturnType)}>",
            ByNameType bt => $"System.Func<{MapType(bt.UnderType)}>",
            _ => "object"
        };
    }

    private string MapTypeNode(TypeNode? node)
    {
        if (node == null) return "object";
        if (node.IsLazy)
        {
            var nonLazy = node with { IsLazy = false };
            return $"System.Func<{MapTypeNode(nonLazy)}>";
        }
        return node.Name switch
        {
            "Int" => "int",
            "Double" => "double",
            "String" => "string",
            "Boolean" => "bool",
            "Unit" => "SSharp.Runtime.Unit",
            "Any" => "object",
            "List" => $"SSharp.Runtime.SSharpList<{string.Join(", ", node.TypeArgs.Select(MapTypeNode))}>",
            "Option" => $"SSharp.Runtime.SSharpOption<{string.Join(", ", node.TypeArgs.Select(MapTypeNode))}>",
            "Some" => $"SSharp.Runtime.Some<{string.Join(", ", node.TypeArgs.Select(MapTypeNode))}>",
            "None" => $"SSharp.Runtime.None<{string.Join(", ", node.TypeArgs.Select(MapTypeNode))}>",
            "Cons" => $"SSharp.Runtime.Cons<{string.Join(", ", node.TypeArgs.Select(MapTypeNode))}>",
            "Nil" => $"SSharp.Runtime.Nil<{string.Join(", ", node.TypeArgs.Select(MapTypeNode))}>",
            _ => node.TypeArgs.Count > 0 
                ? $"{node.Name}<{string.Join(", ", node.TypeArgs.Select(MapTypeNode))}>"
                : node.Name
        };
    }

    // Helper to escape C# reserved keywords in generated identifiers
    private static readonly HashSet<string> CSharpKeywords = new HashSet<string>
    {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
        "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
        "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach",
        "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long",
        "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected",
        "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static",
        "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong",
        "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
    };

    private static string EscapeIdentifier(string name)
    {
        return CSharpKeywords.Contains(name) ? $"@{name}" : name;
    }

    private static string EscapeString(string s)
    {
        return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t") + "\"";
    }

    private string GenerateTailRecBody(Expr expr, string funName, List<Param> funParams, string retType, int indent)
    {
        string spaces = new string(' ', indent);
        switch (expr)
        {
            case BlockExpr block:
                {
                    var sb = new StringBuilder();
                    foreach (var elem in block.Elements)
                    {
                        if (elem is Decl d)
                        {
                            sb.AppendLine(GenerateLocalDecl(d, indent));
                        }
                        else if (elem is Expr e)
                        {
                            sb.AppendLine($"{spaces}{GenerateExpr(e)};");
                        }
                    }
                    if (block.FinalExpr != null)
                    {
                        sb.Append(GenerateTailRecBody(block.FinalExpr, funName, funParams, retType, indent));
                    }
                    else
                    {
                        sb.AppendLine($"{spaces}return SSharp.Runtime.Unit.Instance;");
                    }
                    return sb.ToString();
                }

            case IfExpr ifExpr:
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"{spaces}if ({GenerateExpr(ifExpr.Condition)})");
                    sb.AppendLine($"{spaces}{{");
                    sb.Append(GenerateTailRecBody(ifExpr.ThenBranch, funName, funParams, retType, indent + 4));
                    sb.AppendLine();
                    sb.AppendLine($"{spaces}}}");
                    sb.AppendLine($"{spaces}else");
                    sb.AppendLine($"{spaces}{{");
                    sb.Append(GenerateTailRecBody(ifExpr.ElseBranch, funName, funParams, retType, indent + 4));
                    sb.AppendLine();
                    sb.Append($"{spaces}}}");
                    return sb.ToString();
                }

            case MatchExpr matchExpr:
                {
                    SSharpType matchedType;
                    if (!_resolvedTypes.TryGetValue(matchExpr.Expression, out matchedType!))
                    {
                        matchedType = SSharpType.Any;
                    }

                    var sb = new StringBuilder();
                    sb.AppendLine($"{spaces}switch ({GenerateExpr(matchExpr.Expression)})");
                    sb.AppendLine($"{spaces}{{");
                    foreach (var c in matchExpr.Cases)
                    {
                        string patternStr = GeneratePattern(c.Pattern, matchedType);
                        sb.AppendLine($"{spaces}    case {patternStr}:");
                        sb.AppendLine($"{spaces}        {{");
                        sb.Append(GenerateTailRecBody(c.Body, funName, funParams, retType, indent + 12));
                        sb.AppendLine();
                        sb.AppendLine($"{spaces}        }}");
                    }
                    sb.AppendLine($"{spaces}    default:");
                    sb.AppendLine($"{spaces}        throw new System.InvalidOperationException(\"Pattern match failed\");");
                    sb.Append($"{spaces}}}");
                    return sb.ToString();
                }

            case CallExpr call when IsRecursiveCall(call, funName):
                {
                    var sb = new StringBuilder();
                    var tempVars = new List<string>();
                    var flatArgs = new List<Expr>();
                    GetFlatArguments(call, flatArgs);

                    int count = Math.Min(flatArgs.Count, funParams.Count);

                    // 1. Evaluate arguments and store in temp variables
                    for (int i = 0; i < count; i++)
                    {
                        string tempName = $"_tailrec_temp_{funParams[i].Name}_{i}";
                        string paramType = MapTypeNode(funParams[i].Type);
                        string argExprStr;
                        if (funParams[i].Type.IsLazy)
                        {
                            string underTypeStr = MapTypeNode(funParams[i].Type with { IsLazy = false });
                            argExprStr = $"new System.Func<{underTypeStr}>(() => {GenerateExpr(flatArgs[i])})";
                        }
                        else
                        {
                            argExprStr = GenerateExpr(flatArgs[i]);
                        }
                        sb.AppendLine($"{spaces}{paramType} {tempName} = {argExprStr};");
                        tempVars.Add(tempName);
                    }

                    // 2. Assign temp variables to the actual parameters
                    for (int i = 0; i < count; i++)
                    {
                        sb.AppendLine($"{spaces}{EscapeIdentifier(funParams[i].Name)} = {tempVars[i]};");
                    }

                    // 3. continue the loop
                    sb.Append($"{spaces}continue;");
                    return sb.ToString();
                }

            default:
                return $"{spaces}return {GenerateExpr(expr)};";
        }
    }

    private bool IsRecursiveCall(CallExpr call, string funName)
    {
        if (call.Callee is IdentifierExpr id && id.Name == funName)
        {
            return true;
        }
        if (call.Callee is CallExpr innerCall)
        {
            return IsRecursiveCall(innerCall, funName);
        }
        return false;
    }

    private void GetFlatArguments(CallExpr call, List<Expr> args)
    {
        if (call.Callee is CallExpr innerCall)
        {
            GetFlatArguments(innerCall, args);
        }
        args.AddRange(call.Arguments);
    }
}
