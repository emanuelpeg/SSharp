using System;
using System.Collections.Generic;

namespace SSharp.Compiler;

public class Parser
{
    private readonly List<Token> _tokens;
    private int _current = 0;

    public List<string> Errors { get; } = new();

    private enum Precedence
    {
        None,
        Assignment,  // =
        Match,       // match
        Equality,    // == !=
        Comparison,  // < <= > >=
        Term,        // + -
        Factor,      // * / %
        Unary,       // ! - (prefix)
        Call         // ( [
    }

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    private void Error(Token token, string message)
    {
        if (token.Type == TokenType.EOF)
        {
            Errors.Add($"[{token.Line}:{token.Column}] Error at end: {message}");
        }
        else
        {
            Errors.Add($"[{token.Line}:{token.Column}] Error at '{token.Lexeme}': {message}");
        }
    }

    public Program ParseProgram()
    {
        var decls = new List<Decl>();
        while (!IsAtEnd())
        {
            try
            {
                var decl = ParseDecl();
                if (decl != null)
                {
                    decls.Add(decl);
                }
            }
            catch (Exception)
            {
                Synchronize();
            }
        }
        return new Program(decls);
    }

    private Decl ParseDecl()
    {
        if (Match(TokenType.Import)) return ParseImportDecl();
        if (Check(TokenType.Sealed))
        {
            Token sealedToken = Advance();
            if (Match(TokenType.Trait)) return ParseTraitDecl(sealedToken);
            if (Match(TokenType.Class)) return ParseClassDecl(sealedToken, isCase: false);
            Error(sealedToken, "Expected 'trait' or 'class' after 'sealed'.");
            throw new Exception("Parse error");
        }
        if (Match(TokenType.Trait)) return ParseTraitDecl(null);
        if (Check(TokenType.Class)) return ParseClassDecl(null, isCase: false);
        if (Match(TokenType.Case))
        {
            Token caseToken = Previous();
            if (Match(TokenType.Class))
            {
                return ParseClassDecl(caseToken, isCase: true);
            }
            if (Check(TokenType.Identifier) && Peek().Lexeme == "object")
            {
                Advance(); // Consume identifier 'object'
                Token objName = Consume(TokenType.Identifier, "Expected object name.");
                TypeNode? extendsType = null;
                if (Match(TokenType.Extends))
                {
                    extendsType = ParseType();
                }
                Match(TokenType.Semicolon);
                return new ClassDecl(objName.Lexeme, new List<string>(), new List<Param>(), extendsType, IsCase: true, caseToken.Line, caseToken.Column);
            }
            Error(caseToken, "Expected 'class' or 'object' after 'case'.");
            throw new Exception("Parse error");
        }
        if (Match(TokenType.Def)) return ParseFunDecl();
        if (Match(TokenType.Val)) return ParseValDecl();

        // Top level expression
        Expr expr = ParseExpression();
        Match(TokenType.Semicolon);
        return new ExprDecl(expr);
    }

    private Decl ParseImportDecl()
    {
        Token path = Consume(TokenType.StringLiteral, "Expected string literal after 'import'.");
        Match(TokenType.Semicolon);
        return new ImportDecl((string)path.Value!);
    }

    private Decl ParseTraitDecl(Token? sealedToken)
    {
        Token name = Consume(TokenType.Identifier, "Expected trait name.");
        var typeParams = new List<string>();
        if (Match(TokenType.LBracket))
        {
            do
            {
                Token tp = Consume(TokenType.Identifier, "Expected type parameter name.");
                typeParams.Add(tp.Lexeme);
            } while (Match(TokenType.Comma));
            Consume(TokenType.RBracket, "Expected ']' after type parameters.");
        }
        Match(TokenType.Semicolon);
        int line = sealedToken?.Line ?? name.Line;
        int col = sealedToken?.Column ?? name.Column;
        return new TraitDecl(name.Lexeme, typeParams, line, col);
    }

    private Decl ParseClassDecl(Token? caseOrSealedToken, bool isCase)
    {
        Token name = Consume(TokenType.Identifier, "Expected class name.");
        var typeParams = new List<string>();
        if (Match(TokenType.LBracket))
        {
            do
            {
                Token tp = Consume(TokenType.Identifier, "Expected type parameter name.");
                typeParams.Add(tp.Lexeme);
            } while (Match(TokenType.Comma));
            Consume(TokenType.RBracket, "Expected ']' after type parameters.");
        }

        var constructorParams = new List<Param>();
        if (Match(TokenType.LParen))
        {
            if (!Check(TokenType.RParen))
            {
                do
                {
                    Token paramName = Consume(TokenType.Identifier, "Expected parameter name.");
                    Consume(TokenType.Colon, "Expected ':' after parameter name.");
                    TypeNode paramType = ParseType();
                    constructorParams.Add(new Param(paramName.Lexeme, paramType, paramName.Line, paramName.Column));
                } while (Match(TokenType.Comma));
            }
            Consume(TokenType.RParen, "Expected ')' after constructor parameters.");
        }

        TypeNode? extendsType = null;
        if (Match(TokenType.Extends))
        {
            extendsType = ParseType();
        }

        Match(TokenType.Semicolon);
        int line = caseOrSealedToken?.Line ?? name.Line;
        int col = caseOrSealedToken?.Column ?? name.Column;
        return new ClassDecl(name.Lexeme, typeParams, constructorParams, extendsType, isCase, line, col);
    }

    private Decl ParseFunDecl()
    {
        Token name = Consume(TokenType.Identifier, "Expected function name.");
        var typeParams = new List<string>();
        if (Match(TokenType.LBracket))
        {
            do
            {
                Token tp = Consume(TokenType.Identifier, "Expected type parameter name.");
                typeParams.Add(tp.Lexeme);
            } while (Match(TokenType.Comma));
            Consume(TokenType.RBracket, "Expected ']' after type parameters.");
        }

        Consume(TokenType.LParen, "Expected '(' after function name.");
        var @params = new List<Param>();
        if (!Check(TokenType.RParen))
        {
            do
            {
                Token paramName = Consume(TokenType.Identifier, "Expected parameter name.");
                Consume(TokenType.Colon, "Expected ':' after parameter name.");
                TypeNode paramType = ParseType();
                @params.Add(new Param(paramName.Lexeme, paramType, paramName.Line, paramName.Column));
            } while (Match(TokenType.Comma));
        }
        Consume(TokenType.RParen, "Expected ')' after parameters.");

        TypeNode? returnType = null;
        if (Match(TokenType.Colon))
        {
            returnType = ParseType();
        }

        Consume(TokenType.Assign, "Expected '=' before function body.");
        Expr body = ParseExpression();
        Match(TokenType.Semicolon);

        return new FunDecl(name.Lexeme, typeParams, @params, returnType, body, name.Line, name.Column);
    }

    private Decl ParseValDecl()
    {
        Token name = Consume(TokenType.Identifier, "Expected value name.");
        TypeNode? type = null;
        if (Match(TokenType.Colon))
        {
            type = ParseType();
        }
        Consume(TokenType.Assign, "Expected '=' after value name.");
        Expr value = ParseExpression();
        Match(TokenType.Semicolon);

        return new ValDecl(name.Lexeme, type, value, name.Line, name.Column);
    }

    private TypeNode ParseType()
    {
        Token name = Consume(TokenType.Identifier, "Expected type name.");
        var typeArgs = new List<TypeNode>();
        if (Match(TokenType.LBracket))
        {
            do
            {
                typeArgs.Add(ParseType());
            } while (Match(TokenType.Comma));
            Consume(TokenType.RBracket, "Expected ']' after type arguments.");
        }
        return new TypeNode(name.Lexeme, typeArgs);
    }

    private Expr ParseExpression(Precedence precedence = Precedence.None)
    {
        Token token = Advance();
        Expr expr = ParsePrefix(token);

        while (precedence < GetPrecedence(Peek().Type))
        {
            token = Advance();
            expr = ParseInfix(expr, token);
        }

        return expr;
    }

    private Expr ParsePrefix(Token token)
    {
        switch (token.Type)
        {
            case TokenType.IntLiteral:
            case TokenType.DoubleLiteral:
            case TokenType.StringLiteral:
            case TokenType.True:
            case TokenType.False:
                return new LiteralExpr(token.Value, token.Type, token.Line, token.Column);

            case TokenType.LParen:
                if (IsLambdaAhead())
                {
                    return ParseLambdaExpr(token);
                }
                if (Match(TokenType.RParen))
                {
                    return new UnitExpr(token.Line, token.Column);
                }
                Expr expr = ParseExpression();
                Consume(TokenType.RParen, "Expected ')' after expression.");
                return expr;

            case TokenType.LBrace:
                return ParseBlockExpr(token);

            case TokenType.If:
                return ParseIfExpr(token);

            case TokenType.Identifier:
                if (Peek().Type == TokenType.Arrow)
                {
                    var param = new Param(token.Lexeme, new TypeNode("Any"), token.Line, token.Column);
                    Consume(TokenType.Arrow, "Expected '=>' after lambda parameter.");
                    Expr body = ParseExpression();
                    return new LambdaExpr(new List<Param> { param }, body, token.Line, token.Column);
                }
                return new IdentifierExpr(token.Lexeme, token.Line, token.Column);

            case TokenType.Minus:
            case TokenType.Bang:
                Expr right = ParseExpression(Precedence.Unary);
                return new UnaryExpr(token, right);

            default:
                Error(token, $"Unexpected token '{token.Lexeme}' in expression.");
                return new UnitExpr(token.Line, token.Column);
        }
    }

    private Expr ParseInfix(Expr left, Token op)
    {
        switch (op.Type)
        {
            case TokenType.Plus:
            case TokenType.Minus:
            case TokenType.Asterisk:
            case TokenType.Slash:
            case TokenType.Percent:
            case TokenType.Equals:
            case TokenType.NotEquals:
            case TokenType.LessThan:
            case TokenType.LessOrEqual:
            case TokenType.GreaterThan:
            case TokenType.GreaterOrEqual:
                Precedence prec = GetPrecedence(op.Type);
                Expr right = ParseExpression(prec);
                return new BinaryExpr(left, op, right);

            case TokenType.LParen:
                return ParseCallExpr(left, op);

            case TokenType.LBracket:
                return ParseGenericCallExpr(left, op);

            case TokenType.Match:
                return ParseMatchExpr(left, op);

            default:
                throw new Exception($"Unimplemented infix operator: {op.Type}");
        }
    }

    private Expr ParseCallExpr(Expr callee, Token lParen)
    {
        var args = new List<Expr>();
        if (!Check(TokenType.RParen))
        {
            do
            {
                args.Add(ParseExpression());
            } while (Match(TokenType.Comma));
        }
        Consume(TokenType.RParen, "Expected ')' after call arguments.");
        return new CallExpr(callee, args, new List<TypeNode>(), lParen.Line, lParen.Column);
    }

    private Expr ParseGenericCallExpr(Expr callee, Token lBracket)
    {
        var typeArgs = new List<TypeNode>();
        do
        {
            typeArgs.Add(ParseType());
        } while (Match(TokenType.Comma));
        Consume(TokenType.RBracket, "Expected ']' after generic type arguments.");

        Consume(TokenType.LParen, "Expected '(' after generic type arguments for function call.");
        var args = new List<Expr>();
        if (!Check(TokenType.RParen))
        {
            do
            {
                args.Add(ParseExpression());
            } while (Match(TokenType.Comma));
        }
        Consume(TokenType.RParen, "Expected ')' after call arguments.");
        return new CallExpr(callee, args, typeArgs, lBracket.Line, lBracket.Column);
    }

    private Expr ParseMatchExpr(Expr expression, Token matchToken)
    {
        Consume(TokenType.LBrace, "Expected '{' after match.");
        var cases = new List<MatchCase>();
        while (!Check(TokenType.RBrace) && !IsAtEnd())
        {
            cases.Add(ParseMatchCase());
        }
        Consume(TokenType.RBrace, "Expected '}' at the end of match expression.");
        return new MatchExpr(expression, cases, matchToken.Line, matchToken.Column);
    }

    private MatchCase ParseMatchCase()
    {
        Token caseToken = Consume(TokenType.Case, "Expected 'case' in pattern match.");
        Pattern pattern = ParsePattern();
        Consume(TokenType.Arrow, "Expected '=>' after case pattern.");
        Expr body = ParseExpression();
        Match(TokenType.Semicolon);
        return new MatchCase(pattern, body, caseToken.Line, caseToken.Column);
    }

    private Pattern ParsePattern()
    {
        Token token = Advance();
        switch (token.Type)
        {
            case TokenType.Underscore:
                return new WildcardPattern(token.Line, token.Column);

            case TokenType.IntLiteral:
            case TokenType.DoubleLiteral:
            case TokenType.StringLiteral:
            case TokenType.True:
            case TokenType.False:
                return new LiteralPattern(token.Value, token.Type, token.Line, token.Column);

            case TokenType.Identifier:
                if (Match(TokenType.LParen))
                {
                    var subPatterns = new List<Pattern>();
                    if (!Check(TokenType.RParen))
                    {
                        do
                        {
                            subPatterns.Add(ParsePattern());
                        } while (Match(TokenType.Comma));
                    }
                    Consume(TokenType.RParen, "Expected ')' after constructor subpatterns.");
                    return new ConstructorPattern(token.Lexeme, subPatterns, token.Line, token.Column);
                }
                else
                {
                    if (char.IsUpper(token.Lexeme[0]))
                    {
                        return new ConstructorPattern(token.Lexeme, new List<Pattern>(), token.Line, token.Column);
                    }
                    else
                    {
                        return new IdentifierPattern(token.Lexeme, token.Line, token.Column);
                    }
                }

            default:
                Error(token, "Expected pattern.");
                return new WildcardPattern(token.Line, token.Column);
        }
    }

    private Expr ParseBlockExpr(Token braceToken)
    {
        var elements = new List<ASTNode>();
        Expr? finalExpr = null;

        while (!Check(TokenType.RBrace) && !IsAtEnd())
        {
            if (Check(TokenType.Val) || Check(TokenType.Def))
            {
                elements.Add(ParseDecl());
            }
            else
            {
                Expr expr = ParseExpression();
                if (Check(TokenType.RBrace))
                {
                    finalExpr = expr;
                }
                else
                {
                    elements.Add(expr);
                    Match(TokenType.Semicolon);
                }
            }
        }

        Consume(TokenType.RBrace, "Expected '}' at the end of block expression.");
        return new BlockExpr(elements, finalExpr, braceToken.Line, braceToken.Column);
    }

    private Expr ParseIfExpr(Token ifToken)
    {
        Consume(TokenType.LParen, "Expected '(' after 'if'.");
        Expr cond = ParseExpression();
        Consume(TokenType.RParen, "Expected ')' after 'if' condition.");
        Expr thenBranch = ParseExpression();
        Consume(TokenType.Else, "Expected 'else' after 'if' branch.");
        Expr elseBranch = ParseExpression();
        return new IfExpr(cond, thenBranch, elseBranch, ifToken.Line, ifToken.Column);
    }

    private Expr ParseLambdaExpr(Token lParen)
    {
        var paramsList = new List<Param>();
        if (!Check(TokenType.RParen))
        {
            do
            {
                Token paramName = Consume(TokenType.Identifier, "Expected parameter name.");
                Consume(TokenType.Colon, "Expected ':' after parameter name.");
                TypeNode paramType = ParseType();
                paramsList.Add(new Param(paramName.Lexeme, paramType, paramName.Line, paramName.Column));
            } while (Match(TokenType.Comma));
        }
        Consume(TokenType.RParen, "Expected ')' after lambda parameter list.");
        Consume(TokenType.Arrow, "Expected '=>' after lambda parameters.");
        Expr body = ParseExpression();
        return new LambdaExpr(paramsList, body, lParen.Line, lParen.Column);
    }

    private bool IsLambdaAhead()
    {
        int depth = 1;
        int i = _current;
        while (i < _tokens.Count)
        {
            Token token = _tokens[i];
            if (token.Type == TokenType.LParen) depth++;
            else if (token.Type == TokenType.RParen) depth--;
            
            i++;
            if (depth == 0) break;
        }
        if (i < _tokens.Count)
        {
            return _tokens[i].Type == TokenType.Arrow;
        }
        return false;
    }

    private Precedence GetPrecedence(TokenType type)
    {
        return type switch
        {
            TokenType.Assign => Precedence.Assignment,
            TokenType.Match => Precedence.Match,
            TokenType.Equals or TokenType.NotEquals => Precedence.Equality,
            TokenType.LessThan or TokenType.LessOrEqual or TokenType.GreaterThan or TokenType.GreaterOrEqual => Precedence.Comparison,
            TokenType.Plus or TokenType.Minus => Precedence.Term,
            TokenType.Asterisk or TokenType.Slash or TokenType.Percent => Precedence.Factor,
            TokenType.LParen or TokenType.LBracket => Precedence.Call,
            _ => Precedence.None
        };
    }

    private bool IsAtEnd() => Peek().Type == TokenType.EOF;

    private Token Peek() => _tokens[_current];

    private Token Previous() => _tokens[_current - 1];

    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return Previous();
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private bool Match(TokenType type)
    {
        if (Check(type))
        {
            Advance();
            return true;
        }
        return false;
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();

        Token token = Peek();
        Error(token, message);
        throw new Exception(message);
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous().Type == TokenType.Semicolon) return;

            switch (Peek().Type)
            {
                case TokenType.Import:
                case TokenType.Val:
                case TokenType.Def:
                case TokenType.If:
                case TokenType.Match:
                case TokenType.Trait:
                case TokenType.Class:
                case TokenType.Case:
                    return;
            }

            Advance();
        }
    }
}
