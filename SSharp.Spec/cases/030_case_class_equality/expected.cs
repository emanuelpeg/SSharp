using System;

public record Pair(int a, int b);

public static class Program
{
    public static void Main()
    {
        Pair p1 = new Pair(1, 2);
        Pair p2 = new Pair(1, 2);
        Console.WriteLine(p1 == p2);
    }
}
