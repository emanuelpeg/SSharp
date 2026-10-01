namespace SSharp.Runtime;

/// <summary>
/// Immutable 7-tuple, equivalent to Scala's Tuple7.
/// </summary>
public record SSharpTuple7<A, B, C, D, E, F, G>(A _1, B _2, C _3, D _4, E _5, F _6, G _7)
{
    public override string ToString() => $"({_1},{_2},{_3},{_4},{_5},{_6},{_7})";
}
