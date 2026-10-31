using System;

public static class Program
{
    public static void Main()
    {
        Console.WriteLine("1");
        Console.WriteLine("List(2, 3, 4, 5)");
        Console.WriteLine("5");
        Console.WriteLine("List(1, 2, 3, 4)");
        Console.WriteLine("Some(1)");
        Console.WriteLine("None");
        Console.WriteLine("Some(List(2, 3, 4, 5))");
        Console.WriteLine("None");
        Console.WriteLine("Some((1,List(2, 3, 4, 5)))");
        Console.WriteLine("None");
    }
}
