namespace SSharp.Runtime;

/// <summary>
/// Immutable 4-tuple, equivalent to Scala's Tuple4.
/// </summary>
public record SSharpTuple4<A, B, C, D>(A _1, B _2, C _3, D _4)
{
    public override string ToString() => $"({_1},{_2},{_3},{_4})";
}
