using System;
using System.Collections.Generic;

namespace SSharp.Compiler;

public class Lexer
{
    private readonly string _source;
    private int _start = 0;
    private int _current = 0;
    private int _line = 1;
    private int _column = 1;

    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        { "val", TokenType.Val },
        { "def", TokenType.Def },
        { "if", TokenType.If },
        { "else", TokenType.Else },
        { "match", TokenType.Match },
        { "case", TokenType.Case },
        { "sealed", TokenType.Sealed },
        { "trait", TokenType.Trait },
        { "class", TokenType.Class },
        { "extends", TokenType.Extends },
        { "import", TokenType.Import },
        { "true", TokenType.True },
        { "false", TokenType.False }
    };

    public Lexer(string source)
    {
        _source = source;
    }

    public List<Token> ScanTokens()
    {
        var tokens = new List<Token>();
        while (!IsAtEnd())
        {
            _start = _current;
            var token = ScanToken();
            if (token != null)
            {
                tokens.Add(token);
            }
        }

        tokens.Add(new Token(TokenType.EOF, "", null, _line, _column));
        return tokens;
    }

    private Token? ScanToken()
    {
        char c = Advance();
        switch (c)
        {
            case '(': return MakeToken(TokenType.LParen);
            case ')': return MakeToken(TokenType.RParen);
            case '{': return MakeToken(TokenType.LBrace);
            case '}': return MakeToken(TokenType.RBrace);
            case '[': return MakeToken(TokenType.LBracket);
            case ']': return MakeToken(TokenType.RBracket);
            case ':': return MakeToken(TokenType.Colon);
            case ',': return MakeToken(TokenType.Comma);
            case '.': return MakeToken(TokenType.Dot);
            case ';': return MakeToken(TokenType.Semicolon);
            case '_': return MakeToken(TokenType.Underscore);
            
            case '+': return MakeToken(TokenType.Plus);
            case '-': return MakeToken(TokenType.Minus);
            case '*': return MakeToken(TokenType.Asterisk);
            case '%': return MakeToken(TokenType.Percent);

            case '/':
                if (Match('/'))
                {
                    // A comment goes until the end of the line.
                    while (Peek() != '\n' && !IsAtEnd()) Advance();
                    return null;
                }
                else if (Match('*'))
                {
                    // Block comment
                    while (!IsAtEnd())
                    {
                        if (Peek() == '*' && PeekNext() == '/')
                        {
                            Advance(); // Consume '*'
                            Advance(); // Consume '/'
                            break;
                        }
                        if (Peek() == '\n')
                        {
                            _line++;
                            _column = 1;
                        }
                        Advance();
                    }
                    return null;
                }
                else
                {
                    return MakeToken(TokenType.Slash);
                }

            case '=':
                if (Match('=')) return MakeToken(TokenType.Equals);
                if (Match('>')) return MakeToken(TokenType.Arrow);
                return MakeToken(TokenType.Assign);

            case '!':
                if (Match('=')) return MakeToken(TokenType.NotEquals);
                return MakeToken(TokenType.Bang);

            case '<':
                return MakeToken(Match('=') ? TokenType.LessOrEqual : TokenType.LessThan);

            case '>':
                return MakeToken(Match('=') ? TokenType.GreaterOrEqual : TokenType.GreaterThan);

            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;

            case '\n':
                _line++;
                _column = 1;
                break;

            case '"': return ScanString();

            default:
                if (IsDigit(c))
                {
                    return ScanNumber();
                }
                else if (IsAlpha(c))
                {
                    return ScanIdentifier();
                }
                else
                {
                    return MakeToken(TokenType.Error, null, $"Unexpected character '{c}'");
                }
        }

        return null;
    }

    private Token MakeToken(TokenType type, object? value = null, string? lexeme = null)
    {
        string text = lexeme ?? _source[_start.._current];
        int col = _column - text.Length;
        if (col < 1) col = 1;
        return new Token(type, text, value, _line, col);
    }

    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (_source[_current] != expected) return false;

        _current++;
        _column++;
        return true;
    }

    private char Peek()
    {
        if (IsAtEnd()) return '\0';
        return _source[_current];
    }

    private char PeekNext()
    {
        if (_current + 1 >= _source.Length) return '\0';
        return _source[_current + 1];
    }

    private char Advance()
    {
        char c = _source[_current++];
        _column++;
        return c;
    }

    private bool IsAtEnd() => _current >= _source.Length;

    private bool IsDigit(char c) => c >= '0' && c <= '9';

    private bool IsAlpha(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';

    private bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);

    private Token ScanString()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n')
            {
                _line++;
                _column = 1;
            }
            Advance();
        }

        if (IsAtEnd())
        {
            return MakeToken(TokenType.Error, null, "Unterminated string.");
        }

        // The closing ".
        Advance();

        // Trim the surrounding quotes.
        string value = _source[(_start + 1)..(_current - 1)];
        return MakeToken(TokenType.StringLiteral, value);
    }

    private Token ScanNumber()
    {
        while (IsDigit(Peek())) Advance();

        // Look for a fractional part.
        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            // Consume the "."
            Advance();

            while (IsDigit(Peek())) Advance();

            string lexeme = _source[_start.._current];
            double val = double.Parse(lexeme, System.Globalization.CultureInfo.InvariantCulture);
            return MakeToken(TokenType.DoubleLiteral, val);
        }
        else
        {
            string lexeme = _source[_start.._current];
            int val = int.Parse(lexeme, System.Globalization.CultureInfo.InvariantCulture);
            return MakeToken(TokenType.IntLiteral, val);
        }
    }

    private Token ScanIdentifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();

        string text = _source[_start.._current];
        if (Keywords.TryGetValue(text, out TokenType type))
        {
            return MakeToken(type);
        }
        return MakeToken(TokenType.Identifier);
    }
}
