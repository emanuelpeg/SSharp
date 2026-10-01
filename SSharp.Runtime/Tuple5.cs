namespace SSharp.Runtime;

/// <summary>
/// Immutable 5-tuple, equivalent to Scala's Tuple5.
/// </summary>
public record SSharpTuple5<A, B, C, D, E>(A _1, B _2, C _3, D _4, E _5)
{
    public override string ToString() => $"({_1},{_2},{_3},{_4},{_5})";
}
