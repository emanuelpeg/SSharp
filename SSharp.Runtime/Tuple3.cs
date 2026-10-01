namespace SSharp.Runtime;

/// <summary>
/// Immutable 3-tuple, equivalent to Scala's Tuple3.
/// </summary>
public record SSharpTuple3<A, B, C>(A _1, B _2, C _3)
{
    public override string ToString() => $"({_1},{_2},{_3})";
}
