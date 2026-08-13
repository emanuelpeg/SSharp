using System;

public static class Program
{
    public static string greet(string name) => name switch {
        "Alice" => "Hello Alice!",
        "Bob" => "Hello Bob!",
        _ => "Hello stranger!"
    };

    public static void Main()
    {
        Console.WriteLine(greet("Alice"));
        Console.WriteLine(greet("Charlie"));
    }
}
