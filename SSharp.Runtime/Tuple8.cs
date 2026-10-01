namespace SSharp.Runtime;

/// <summary>
/// Immutable 8-tuple, equivalent to Scala's Tuple8.
/// </summary>
public record SSharpTuple8<A, B, C, D, E, F, G, H>(A _1, B _2, C _3, D _4, E _5, F _6, G _7, H _8)
{
    public override string ToString() => $"({_1},{_2},{_3},{_4},{_5},{_6},{_7},{_8})";
}
