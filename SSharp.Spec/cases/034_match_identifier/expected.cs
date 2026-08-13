using System;

public static class Program
{
    public static string describe(int x) => "number is " + x;

    public static void Main()
    {
        Console.WriteLine(describe(42));
    }
}
