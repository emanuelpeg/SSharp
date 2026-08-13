using System;

public static class Program
{
    public static int applyTwice(Func<int, int> f, int x) => f(f(x));

    public static void Main()
    {
        Func<int, int> inc = (x) => x + 1;
        Console.WriteLine(applyTwice(inc, 5));
    }
}
