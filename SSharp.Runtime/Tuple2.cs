namespace SSharp.Runtime;

/// <summary>
/// Immutable pair (2-tuple), equivalent to Scala's Tuple2.
/// Used as Map key-value pairs: Map(("a", 1), ("b", 2))
/// </summary>
public record SSharpTuple2<A, B>(A _1, B _2)
{
    public override string ToString() => $"({_1},{_2})";
}
