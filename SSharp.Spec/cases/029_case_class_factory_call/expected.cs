using System;

public record Box(int value);

public static class Program
{
    public static void Main()
    {
        Box b = new Box(42);
        Console.WriteLine(b.value);
    }
}
