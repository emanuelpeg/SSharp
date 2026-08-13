using System;

public static class Program
{
    private static readonly Lazy<string> _lazy_message = new Lazy<string>(() => {
        Console.WriteLine("evaluated");
        return "hello";
    });
    public static string message => _lazy_message.Value;

    public static void Main()
    {
        Console.WriteLine("before");
        Console.WriteLine(message);
        Console.WriteLine(message);
    }
}
