namespace SSharp.Runtime;

/// <summary>
/// Immutable 6-tuple, equivalent to Scala's Tuple6.
/// </summary>
public record SSharpTuple6<A, B, C, D, E, F>(A _1, B _2, C _3, D _4, E _5, F _6)
{
    public override string ToString() => $"({_1},{_2},{_3},{_4},{_5},{_6})";
}
