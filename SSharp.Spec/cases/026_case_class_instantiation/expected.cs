using System;

public record Point(int x, int y);

public static class Program
{
    public static void Main()
    {
        Point p = new Point(3, 4);
        Console.WriteLine(p.x + p.y);
    }
}
