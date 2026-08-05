using System.Collections.Generic;

namespace SSharp.Compiler;

public record TypeNode(string Name, List<TypeNode> TypeArgs, bool IsLazy = false)
{
    public TypeNode(string name) : this(name, new List<TypeNode>(), false) { }
    public override string ToString() => IsLazy ? $"=> {Name}" : (TypeArgs.Count == 0 ? Name : $"{Name}[{string.Join(", ", TypeArgs)}]");
}

public abstract record ASTNode;

// Declarations
public abstract record Decl : ASTNode;

public record Program(List<Decl> Decls) : ASTNode;

public record ImportDecl(string Path) : Decl;

public record ExprDecl(Expr Expression) : Decl;

public record ValDecl(string Name, TypeNode? Type, Expr Value, int Line, int Column, bool IsLazy = false) : Decl;

public enum Variance
{
    Invariant,
    Covariant,
    Contravariant
}

public record TypeParam(string Name, Variance Variance = Variance.Invariant)
{
    public override string ToString() => Variance switch
    {
        Variance.Covariant => $"+{Name}",
        Variance.Contravariant => $"-{Name}",
        _ => Name
    };

    public static implicit operator TypeParam(string name) => new(name);
}

public record Param(string Name, TypeNode Type, int Line, int Column);

public record FunDecl(string Name, List<TypeParam> TypeParams, List<Param> Params, TypeNode? ReturnType, Expr Body, int Line, int Column, bool IsTailRec = false) : Decl;

public record TraitDecl(string Name, List<TypeParam> TypeParams, int Line, int Column) : Decl;

public record ClassDecl(string Name, List<TypeParam> TypeParams, List<Param> ConstructorParams, TypeNode? ExtendsType, bool IsCase, int Line, int Column) : Decl;

// Expressions
public abstract record Expr : ASTNode;

public record LiteralExpr(object? Value, TokenType Type, int Line, int Column) : Expr;

public record UnitExpr(int Line, int Column) : Expr;

public record IdentifierExpr(string Name, int Line, int Column) : Expr;

public record BinaryExpr(Expr Lhs, Token Op, Expr Rhs) : Expr;

public record UnaryExpr(Token Op, Expr Expr) : Expr;

public record BlockExpr(List<ASTNode> Elements, Expr? FinalExpr, int Line, int Column) : Expr;

public record IfExpr(Expr Condition, Expr ThenBranch, Expr ElseBranch, int Line, int Column) : Expr;

public record CallExpr(Expr Callee, List<Expr> Arguments, List<TypeNode> TypeArgs, int Line, int Column) : Expr;

public record LambdaExpr(List<Param> Params, Expr Body, int Line, int Column) : Expr;

public record MatchExpr(Expr Expression, List<MatchCase> Cases, int Line, int Column) : Expr;

/// <summary>
/// Represents dot-notation member access: receiver.Member or receiver.Method(args).
/// If Arguments is null, it is a property access (zero-arg call).
/// If Arguments is non-null, it is a method call with those arguments.
/// </summary>
public record MemberAccessExpr(Expr Receiver, string Member, List<TypeNode> TypeArgs, List<Expr>? Arguments, int Line, int Column) : Expr;


// Patterns and Match Cases
public record MatchCase(Pattern Pattern, Expr Body, int Line, int Column);

public abstract record Pattern : ASTNode;

public record WildcardPattern(int Line, int Column) : Pattern;

public record LiteralPattern(object? Value, TokenType Type, int Line, int Column) : Pattern;

public record IdentifierPattern(string Name, int Line, int Column) : Pattern;

public record ConstructorPattern(string Name, List<Pattern> SubPatterns, int Line, int Column) : Pattern;
