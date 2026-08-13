using System;

public static class Program
{
    public static bool isZero(int x) => x switch {
        0 => true,
        _ => false
    };

    public static void Main()
    {
        Console.WriteLine(isZero(0));
        Console.WriteLine(isZero(10));
    }
}
