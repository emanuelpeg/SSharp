namespace SSharp.Compiler;

public enum TokenType
{
    // Literals
    Identifier,
    IntLiteral,
    DoubleLiteral,
    StringLiteral,

    // Keywords
    Val,
    Lazy,
    Def,
    If,
    Else,
    Match,
    Case,
    Sealed,
    Trait,
    Class,
    Extends,
    Import,
    True,
    False,
    For,
    Yield,
    TypeAlias,  // type
    Data,       // data

    // Operators
    Plus,       // +
    Minus,      // -
    Asterisk,   // *
    Slash,      // /
    Percent,    // %
    Assign,     // =
    Equals,     // ==
    NotEquals,  // !=
    LessThan,   // <
    LessOrEqual,// <=
    GreaterThan,// >
    GreaterOrEqual,// >=
    Arrow,      // =>
    ArrowThin,  // ->
    LeftArrow,  // <-
    Bang,       // !
    At,         // @
    Pipe,       // |>
    Bar,        // |

    // Delimiters
    LParen,     // (
    RParen,     // )
    LBrace,     // {
    RBrace,     // }
    LBracket,   // [
    RBracket,   // ]
    Colon,      // :
    ColonColon, // ::
    Comma,      // ,
    Dot,        // .
    Semicolon,  // ;
    Underscore, // _

    // Special
    EOF,
    Error
}

public record Token(TokenType Type, string Lexeme, object? Value, int Line, int Column);
