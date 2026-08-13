using System;

public interface OptionVal {}
public record SomeVal(int v) : OptionVal;
public record NoneVal : OptionVal {
    private NoneVal() {}
    public static NoneVal Instance { get; } = new NoneVal();
}

public static class Program
{
    public static int getOrDefault(OptionVal opt) => opt switch {
        SomeVal(var x) => x,
        NoneVal => 0,
        _ => throw new InvalidOperationException()
    };

    public static void Main()
    {
        Console.WriteLine(getOrDefault(new SomeVal(10)));
        Console.WriteLine(getOrDefault(NoneVal.Instance));
    }
}
