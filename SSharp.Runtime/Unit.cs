namespace SSharp.Runtime;

public record Unit
{
    private Unit() {}
    public static Unit Instance { get; } = new Unit();
    public override string ToString() => "()";
}
