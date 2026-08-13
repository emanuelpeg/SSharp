using System;

public static class Program
{
    public static bool isEven(int n) => n % 2 == 0;

    public static void Main()
    {
        Console.WriteLine(isEven(4));
        Console.WriteLine(isEven(5));
    }
}
