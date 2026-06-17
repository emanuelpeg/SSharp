using System;
using System.Collections.Generic;

namespace SSharp.Compiler;

public abstract record SSharpType
{
    public static SSharpType Int { get; } = new PrimitiveType("Int");
    public static SSharpType Double { get; } = new PrimitiveType("Double");
    public static SSharpType String { get; } = new PrimitiveType("String");
    public static SSharpType Boolean { get; } = new PrimitiveType("Boolean");
    public static SSharpType Unit { get; } = new PrimitiveType("Unit");
    public static SSharpType Any { get; } = new PrimitiveType("Any");
}

public record PrimitiveType(string Name) : SSharpType
{
    public override string ToString() => Name;
}

public record GenericType(string Name, List<SSharpType> TypeArgs) : SSharpType
{
    public override string ToString() => TypeArgs.Count == 0 ? Name : $"{Name}[{string.Join(", ", TypeArgs)}]";
}

public record FunctionType(List<SSharpType> ParamTypes, SSharpType ReturnType) : SSharpType
{
    public override string ToString() => $"({string.Join(", ", ParamTypes)}) => {ReturnType}";
}

public class TypeChecker
{
    private class Env
    {
        private readonly Env? _parent;
        private readonly Dictionary<string, SSharpType> _bindings = new();

        public Env(Env? parent = null)
        {
            _parent = parent;
        }

        public void Define(string name, SSharpType type)
        {
            _bindings[name] = type;
        }

        public bool Lookup(string name, out SSharpType type)
        {
            if (_bindings.TryGetValue(name, out type!))
            {
                return true;
            }
            if (_parent != null)
            {
                return _parent.Lookup(name, out type);
            }
            return false;
        }
    }

    private Env _env = new();
    public List<string> Errors { get; } = new();

    private readonly Dictionary<string, ClassDecl> _classes = new();
    private readonly Dictionary<string, TraitDecl> _traits = new();

    public TypeChecker()
    {
        // Add predefined functions from Runtime Predef
        _env.Define("print", new FunctionType(new List<SSharpType> { SSharpType.Any }, SSharpType.Unit));
        _env.Define("println", new FunctionType(new List<SSharpType> { SSharpType.Any }, SSharpType.Unit));
        _env.Define("readLine", new FunctionType(new List<SSharpType>(), SSharpType.String));

        // Register List and Option in types if needed
        // Register List trait
        _traits["List"] = new TraitDecl("List", new List<string> { "A" }, 0, 0);

        // Register Cons class
        var consParams = new List<Param>
        {
            new Param("head", new TypeNode("A"), 0, 0),
            new Param("tail", new TypeNode("List", new List<TypeNode> { new TypeNode("A") }), 0, 0)
        };
        _classes["Cons"] = new ClassDecl("Cons", new List<string> { "A" }, consParams, new TypeNode("List", new List<TypeNode> { new TypeNode("A") }), IsCase: true, 0, 0);

        var consParamTypes = new List<SSharpType>
        {
            new PrimitiveType("A"),
            new GenericType("List", new List<SSharpType> { new PrimitiveType("A") })
        };
        var consRetType = new GenericType("List", new List<SSharpType> { new PrimitiveType("A") });
        _env.Define("Cons", new FunctionType(consParamTypes, consRetType));

        // Register Nil case object
        _classes["Nil"] = new ClassDecl("Nil", new List<string>(), new List<Param>(), new TypeNode("List", new List<TypeNode> { new TypeNode("Any") }), IsCase: true, 0, 0);
        _env.Define("Nil", new GenericType("List", new List<SSharpType> { SSharpType.Any }));
        _env.Define("List", new PrimitiveType("ListFactory"));

        // Register Option trait
        _traits["Option"] = new TraitDecl("Option", new List<string> { "A" }, 0, 0);

        // Register Some class
        var someParams = new List<Param>
        {
            new Param("value", new TypeNode("A"), 0, 0)
        };
        _classes["Some"] = new ClassDecl("Some", new List<string> { "A" }, someParams, new TypeNode("Option", new List<TypeNode> { new TypeNode("A") }), IsCase: true, 0, 0);

        var someParamTypes = new List<SSharpType> { new PrimitiveType("A") };
        var someRetType = new GenericType("Option", new List<SSharpType> { new PrimitiveType("A") });
        _env.Define("Some", new FunctionType(someParamTypes, someRetType));

        // Register None case object
        _classes["None"] = new ClassDecl("None", new List<string>(), new List<Param>(), new TypeNode("Option", new List<TypeNode> { new TypeNode("Any") }), IsCase: true, 0, 0);
        _env.Define("None", new GenericType("Option", new List<SSharpType> { SSharpType.Any }));
    }

    private void Error(int line, int col, string message)
    {
        Errors.Add($"[{line}:{col}] Type Error: {message}");
    }

    public void Check(Program program)
    {
        // First pass: Register all traits and classes
        foreach (var decl in program.Decls)
        {
            if (decl is TraitDecl trait)
            {
                _traits[trait.Name] = trait;
            }
            else if (decl is ClassDecl cls)
            {
                _classes[cls.Name] = cls;
                
                // If it is a case class, register constructor function/method in env
                // e.g. Cons(head: T, tail: List[T]) has constructor function
                if (cls.IsCase && cls.ConstructorParams.Count > 0)
                {
                    // For generics, we can represent constructor type
                    // In SSharp, case class constructor can be called like a function
                    // Let's assume generic params are mapped. SSharp Type Checker simplified:
                    // We bind constructor name to a function type or special constructor type.
                    // For simplified v1, we can bind to a FunctionType taking the constructor params
                    // mapping parameter types to SSharpType.
                    var paramTypes = new List<SSharpType>();
                    foreach (var p in cls.ConstructorParams)
                    {
                        paramTypes.Add(ResolveType(p.Type));
                    }
                    var retType = cls.TypeParams.Count > 0
                        ? new GenericType(cls.Name, cls.TypeParams.ConvertAll(tp => (SSharpType)new PrimitiveType(tp)))
                        : (SSharpType)new PrimitiveType(cls.Name);
                    
                    _env.Define(cls.Name, new FunctionType(paramTypes, retType));
                }
                else if (cls.IsCase && cls.ConstructorParams.Count == 0)
                {
                    // Case object Nil: registered as value Nil: List[Nothing] / List[Any]
                    var retType = cls.ExtendsType != null ? ResolveType(cls.ExtendsType) : new PrimitiveType(cls.Name);
                    _env.Define(cls.Name, retType);
                }
            }
        }

        // Second pass: Check declarations
        foreach (var decl in program.Decls)
        {
            CheckDecl(decl);
        }
    }

    private void CheckDecl(Decl decl)
    {
        switch (decl)
        {
            case ImportDecl:
                break; // Handled dynamically or by compiler code generation

            case TraitDecl:
                break; // Registered in first pass

            case ClassDecl:
                break; // Registered in first pass

            case ValDecl valDecl:
                SSharpType valType = CheckExpr(valDecl.Value);
                if (valDecl.Type != null)
                {
                    SSharpType expectedType = ResolveType(valDecl.Type);
                    if (!IsSubtype(valType, expectedType))
                    {
                        Error(valDecl.Line, valDecl.Column, $"Type mismatch: Val '{valDecl.Name}' expected {expectedType}, but got {valType}.");
                    }
                    _env.Define(valDecl.Name, expectedType);
                }
                else
                {
                    _env.Define(valDecl.Name, valType);
                }
                break;

            case FunDecl funDecl:
                // Create function environment
                var prevEnv = _env;
                _env = new Env(prevEnv);

                // Define type parameters in local env as any/primitive types
                foreach (var tp in funDecl.TypeParams)
                {
                    _env.Define(tp, new PrimitiveType(tp));
                }

                // Define params in function environment
                var paramTypesList = new List<SSharpType>();
                foreach (var param in funDecl.Params)
                {
                    SSharpType pType = ResolveType(param.Type);
                    _env.Define(param.Name, pType);
                    paramTypesList.Add(pType);
                }

                // If return type is specified, resolve it
                SSharpType? expectedRetType = funDecl.ReturnType != null ? ResolveType(funDecl.ReturnType) : null;

                // Define function in outer environment beforehand to support recursion
                if (expectedRetType != null)
                {
                    prevEnv.Define(funDecl.Name, new FunctionType(paramTypesList, expectedRetType));
                }

                // Check function body
                SSharpType bodyType = CheckExpr(funDecl.Body);

                if (expectedRetType != null)
                {
                    if (!IsSubtype(bodyType, expectedRetType))
                    {
                        Error(funDecl.Line, funDecl.Column, $"Type mismatch in function '{funDecl.Name}': expected return type {expectedRetType}, but got {bodyType}.");
                    }
                }
                else
                {
                    expectedRetType = bodyType;
                }

                // Restore environment
                _env = prevEnv;

                // Bind function in outer environment
                _env.Define(funDecl.Name, new FunctionType(paramTypesList, expectedRetType));
                break;

            case ExprDecl exprDecl:
                CheckExpr(exprDecl.Expression);
                break;

            default:
                throw new Exception($"Unknown declaration type: {decl.GetType().Name}");
        }
    }

    public Dictionary<Expr, SSharpType> ResolvedTypes { get; } = new();

    private SSharpType CheckExpr(Expr expr)
    {
        SSharpType result = CheckExprInternal(expr);
        ResolvedTypes[expr] = result;
        return result;
    }

    private SSharpType CheckExprInternal(Expr expr)
    {
        switch (expr)
        {
            case LiteralExpr lit:
                return lit.Type switch
                {
                    TokenType.IntLiteral => SSharpType.Int,
                    TokenType.DoubleLiteral => SSharpType.Double,
                    TokenType.StringLiteral => SSharpType.String,
                    TokenType.True or TokenType.False => SSharpType.Boolean,
                    _ => SSharpType.Any
                };

            case UnitExpr:
                return SSharpType.Unit;

            case IdentifierExpr id:
                if (_env.Lookup(id.Name, out SSharpType type))
                {
                    return type;
                }
                // Check if it matches a case object or class name
                if (_classes.TryGetValue(id.Name, out var cls) && cls.IsCase && cls.ConstructorParams.Count == 0)
                {
                    return new PrimitiveType(cls.Name);
                }
                Error(id.Line, id.Column, $"Identifier '{id.Name}' not found in current scope.");
                return SSharpType.Any;

            case BinaryExpr bin:
                SSharpType lhsType = CheckExpr(bin.Lhs);
                SSharpType rhsType = CheckExpr(bin.Rhs);

                // Arithmetic operators
                if (bin.Op.Type is TokenType.Plus or TokenType.Minus or TokenType.Asterisk or TokenType.Slash or TokenType.Percent)
                {
                    if (lhsType == SSharpType.Int && rhsType == SSharpType.Int) return SSharpType.Int;
                    if (lhsType == SSharpType.Double || rhsType == SSharpType.Double)
                    {
                        if ((lhsType == SSharpType.Int || lhsType == SSharpType.Double) &&
                            (rhsType == SSharpType.Int || rhsType == SSharpType.Double))
                        {
                            return SSharpType.Double;
                        }
                    }
                    if (bin.Op.Type == TokenType.Plus && (lhsType == SSharpType.String || rhsType == SSharpType.String))
                    {
                        return SSharpType.String; // String concatenation
                    }
                    Error(bin.Op.Line, bin.Op.Column, $"Operator '{bin.Op.Lexeme}' cannot be applied to types {lhsType} and {rhsType}.");
                    return SSharpType.Any;
                }

                // Comparison operators
                if (bin.Op.Type is TokenType.LessThan or TokenType.LessOrEqual or TokenType.GreaterThan or TokenType.GreaterOrEqual)
                {
                    if ((lhsType == SSharpType.Int || lhsType == SSharpType.Double) &&
                        (rhsType == SSharpType.Int || rhsType == SSharpType.Double))
                    {
                        return SSharpType.Boolean;
                    }
                    Error(bin.Op.Line, bin.Op.Column, $"Operator '{bin.Op.Lexeme}' cannot be applied to types {lhsType} and {rhsType}.");
                    return SSharpType.Boolean;
                }

                // Equality operators
                if (bin.Op.Type is TokenType.Equals or TokenType.NotEquals)
                {
                    return SSharpType.Boolean;
                }

                Error(bin.Op.Line, bin.Op.Column, $"Unknown binary operator '{bin.Op.Lexeme}'.");
                return SSharpType.Any;

            case UnaryExpr unary:
                SSharpType operandType = CheckExpr(unary.Expr);
                if (unary.Op.Type == TokenType.Minus)
                {
                    if (operandType == SSharpType.Int) return SSharpType.Int;
                    if (operandType == SSharpType.Double) return SSharpType.Double;
                    Error(unary.Op.Line, unary.Op.Column, $"Operator '-' cannot be applied to type {operandType}.");
                    return SSharpType.Any;
                }
                if (unary.Op.Type == TokenType.Bang)
                {
                    if (operandType == SSharpType.Boolean) return SSharpType.Boolean;
                    Error(unary.Op.Line, unary.Op.Column, $"Operator '!' cannot be applied to type {operandType}.");
                    return SSharpType.Boolean;
                }
                Error(unary.Op.Line, unary.Op.Column, $"Unknown unary operator '{unary.Op.Lexeme}'.");
                return SSharpType.Any;

            case BlockExpr block:
                var blockEnv = new Env(_env);
                var prevEnv = _env;
                _env = blockEnv;

                foreach (var elem in block.Elements)
                {
                    if (elem is Decl d) CheckDecl(d);
                    else if (elem is Expr e) CheckExpr(e);
                }

                SSharpType result = block.FinalExpr != null ? CheckExpr(block.FinalExpr) : SSharpType.Unit;
                _env = prevEnv;
                return result;

            case IfExpr condExpr:
                SSharpType condType = CheckExpr(condExpr.Condition);
                if (condType != SSharpType.Boolean)
                {
                    Error(condExpr.Line, condExpr.Column, $"If condition must be Boolean, but got {condType}.");
                }
                SSharpType thenType = CheckExpr(condExpr.ThenBranch);
                SSharpType elseType = CheckExpr(condExpr.ElseBranch);

                // Both branches must align. If they don't, return common supertype (e.g. Any)
                if (IsSubtype(thenType, elseType)) return elseType;
                if (IsSubtype(elseType, thenType)) return thenType;
                return SSharpType.Any;

            case CallExpr call:
                SSharpType calleeType = CheckExpr(call.Callee);
                if (calleeType is PrimitiveType factoryPt && factoryPt.Name == "ListFactory")
                {
                    SSharpType elementCommonType = SSharpType.Any;
                    if (call.Arguments.Count > 0)
                    {
                        elementCommonType = CheckExpr(call.Arguments[0]);
                        for (int i = 1; i < call.Arguments.Count; i++)
                        {
                            SSharpType argType = CheckExpr(call.Arguments[i]);
                            if (IsSubtype(argType, elementCommonType)) { }
                            else if (IsSubtype(elementCommonType, argType))
                            {
                                elementCommonType = argType;
                            }
                            else
                            {
                                elementCommonType = SSharpType.Any;
                            }
                        }
                    }
                    return new GenericType("List", new List<SSharpType> { elementCommonType });
                }
                if (calleeType is FunctionType funType)
                {
                    int expectedCount = funType.ParamTypes.Count;
                    int actualCount = call.Arguments.Count;

                    if (actualCount > expectedCount)
                    {
                        Error(call.Line, call.Column, $"Function expected {expectedCount} arguments, but got {actualCount}.");
                    }

                    for (int i = 0; i < Math.Min(expectedCount, actualCount); i++)
                    {
                        SSharpType argType = CheckExpr(call.Arguments[i]);
                        // If callee has generic parameters that we're passing, we can skip strict checks or bind type variables
                        // For a simple v1, we check subtype or allow matching generic parameter names (like T, A)
                        SSharpType expected = funType.ParamTypes[i];
                        if (expected is PrimitiveType pt && pt.Name.Length == 1 && char.IsUpper(pt.Name[0]))
                        {
                            // This is a type variable (e.g. A, T) - accept any type here (simplifying type inference)
                            continue;
                        }
                        if (!IsSubtype(argType, expected))
                        {
                            Error(call.Line, call.Column, $"Argument {i + 1} type mismatch: expected {expected}, but got {argType}.");
                        }
                    }

                    if (actualCount < expectedCount)
                    {
                        // Partial application: return a function taking the remaining parameters
                        var remainingParams = funType.ParamTypes.GetRange(actualCount, expectedCount - actualCount);
                        return new FunctionType(remainingParams, funType.ReturnType);
                    }

                    return funType.ReturnType;
                }
                // If it is a generic constructor check (e.g. Cons(1, Nil)), or a direct Type constructor call
                // represented by custom PrimitiveType (like List or Cons)
                if (calleeType is PrimitiveType ptType && _classes.TryGetValue(ptType.Name, out var targetCls))
                {
                    return new GenericType(targetCls.Name, new List<SSharpType>());
                }
                Error(call.Line, call.Column, $"Callee expression of type {calleeType} is not callable.");
                return SSharpType.Any;

            case LambdaExpr lambda:
                var lambdaEnv = new Env(_env);
                var prevLambdaEnv = _env;
                _env = lambdaEnv;

                var paramTypes = new List<SSharpType>();
                foreach (var p in lambda.Params)
                {
                    SSharpType pt = ResolveType(p.Type);
                    _env.Define(p.Name, pt);
                    paramTypes.Add(pt);
                }

                SSharpType returnType = CheckExpr(lambda.Body);
                _env = prevLambdaEnv;
                return new FunctionType(paramTypes, returnType);

            case MatchExpr match:
                SSharpType matchType = CheckExpr(match.Expression);
                SSharpType? casesCommonType = null;

                foreach (var c in match.Cases)
                {
                    // Pattern binds variables in a case-specific environment scope
                    var caseEnv = new Env(_env);
                    var prevCaseEnv = _env;
                    _env = caseEnv;

                    BindPatternVariables(c.Pattern, matchType);

                    SSharpType caseBodyType = CheckExpr(c.Body);
                    _env = prevCaseEnv;

                    if (casesCommonType == null)
                    {
                        casesCommonType = caseBodyType;
                    }
                    else if (!IsSubtype(caseBodyType, casesCommonType))
                    {
                        if (IsSubtype(casesCommonType, caseBodyType))
                        {
                            casesCommonType = caseBodyType;
                        }
                        else
                        {
                            casesCommonType = SSharpType.Any;
                        }
                    }
                }
                return casesCommonType ?? SSharpType.Unit;

            default:
                throw new Exception($"Unknown expression type: {expr.GetType().Name}");
        }
    }

    private void BindPatternVariables(Pattern pattern, SSharpType type)
    {
        switch (pattern)
        {
            case WildcardPattern:
                break;

            case LiteralPattern:
                break;

            case IdentifierPattern idPat:
                // Lowercase identifier binds the value of the match type
                _env.Define(idPat.Name, type);
                break;

            case ConstructorPattern consPat:
                // e.g. Cons(head, tail) matching against List[Int]
                // We lookup the case class definition
                if (_classes.TryGetValue(consPat.Name, out var clsDecl))
                {
                    // If matching List[Int] with Cons(h, t):
                    // head type is Int, tail type is List[Int].
                    // SSharp type checker retrieves constructor parameter types and binds subpatterns.
                    // For simple v1, we extract types from the case class declaration
                    // If the matched type is a GenericType (like List[Int]), we map the type parameters (like A -> Int)
                    var typeMap = new Dictionary<string, SSharpType>();
                    if (type is GenericType gt && clsDecl.ExtendsType != null && clsDecl.ExtendsType.TypeArgs.Count > 0)
                    {
                        // Match class type parameters
                        // Cons[A](head: A, tail: List[A]) extends List[A]
                        // Match Type is List[Int] -> A maps to Int
                        for (int i = 0; i < Math.Min(clsDecl.ExtendsType.TypeArgs.Count, gt.TypeArgs.Count); i++)
                        {
                            var extendsArg = clsDecl.ExtendsType.TypeArgs[i];
                            typeMap[extendsArg.Name] = gt.TypeArgs[i];
                        }
                    }

                    for (int i = 0; i < Math.Min(consPat.SubPatterns.Count, clsDecl.ConstructorParams.Count); i++)
                    {
                        var paramDecl = clsDecl.ConstructorParams[i];
                        SSharpType resolvedParamType = ResolveType(paramDecl.Type);
                        if (resolvedParamType is PrimitiveType pt && typeMap.TryGetValue(pt.Name, out var mappedType))
                        {
                            resolvedParamType = mappedType;
                        }
                        else if (resolvedParamType is GenericType genType)
                        {
                            // Map type parameters inside generic arguments e.g., List[A] -> List[Int]
                            var mappedArgs = new List<SSharpType>();
                            foreach (var arg in genType.TypeArgs)
                            {
                                if (arg is PrimitiveType argPt && typeMap.TryGetValue(argPt.Name, out var mArg))
                                {
                                    mappedArgs.Add(mArg);
                                }
                                else
                                {
                                    mappedArgs.Add(arg);
                                }
                            }
                            resolvedParamType = new GenericType(genType.Name, mappedArgs);
                        }

                        BindPatternVariables(consPat.SubPatterns[i], resolvedParamType);
                    }
                }
                break;
        }
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

    private bool IsSubtype(SSharpType sub, SSharpType super)
    {
        if (super == SSharpType.Any) return true;
        if (super is PrimitiveType pt && pt.Name.Length == 1 && char.IsUpper(pt.Name[0])) return true;
        if (sub == super) return true;

        if (sub is PrimitiveType subPt && super is PrimitiveType superPt)
        {
            // Number conversion
            if (subPt == SSharpType.Int && superPt == SSharpType.Double) return true;

            // Class inheritance
            if (_classes.TryGetValue(subPt.Name, out var cls) && cls.ExtendsType != null)
            {
                if (cls.ExtendsType.Name == superPt.Name) return true;
            }
        }

        if (sub is GenericType subGt && super is GenericType superGt)
        {
            if (subGt.Name != superGt.Name)
            {
                // Cons[T] is a subtype of List[T]
                if (_classes.TryGetValue(subGt.Name, out var cls) && cls.ExtendsType != null)
                {
                    if (cls.ExtendsType.Name == superGt.Name) return true;
                }
                return false;
            }
            if (subGt.TypeArgs.Count != superGt.TypeArgs.Count) return false;
            for (int i = 0; i < subGt.TypeArgs.Count; i++)
            {
                // Covariant generic parameters by default (like Option[+A] or List[+A])
                if (!IsSubtype(subGt.TypeArgs[i], superGt.TypeArgs[i])) return false;
            }
            return true;
        }

        return false;
    }
}
