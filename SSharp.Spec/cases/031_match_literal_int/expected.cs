using System;

public static class Program
{
    public static string check(int x) => x switch {
        1 => "one",
        2 => "two",
        _ => "other"
    };

    public static void Main()
    {
        Console.WriteLine(check(1));
        Console.WriteLine(check(2));
        Console.WriteLine(check(5));
    }
}
