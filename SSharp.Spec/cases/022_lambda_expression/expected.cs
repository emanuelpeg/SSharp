using System;

public static class Program
{
    public static void Main()
    {
        Func<int, int> sq = (x) => x * x;
        Console.WriteLine(sq(6));
    }
}
