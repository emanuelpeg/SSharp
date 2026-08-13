using System;

public static class Program
{
    public static Func<int, int> makeMultiplier(int factor) => (x) => x * factor;

    public static void Main()
    {
        Func<int, int> triple = makeMultiplier(3);
        Console.WriteLine(triple(7));
    }
}
