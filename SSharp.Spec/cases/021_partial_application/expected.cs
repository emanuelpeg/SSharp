using System;

public static class Program
{
    public static int sum(int a, int b) => a + b;

    public static void Main()
    {
        Func<int, int> addFive = (b) => sum(5, b);
        Console.WriteLine(addFive(10));
    }
}
