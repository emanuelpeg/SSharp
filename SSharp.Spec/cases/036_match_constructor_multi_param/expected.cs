using System;

public record Person(string name, int age);

public static class Program
{
    public static bool isAdult(Person p) => p switch {
        Person(var name, var age) => age >= 18,
        _ => throw new InvalidOperationException()
    };

    public static void Main()
    {
        Console.WriteLine(isAdult(new Person("Bob", 20)));
        Console.WriteLine(isAdult(new Person("Kid", 10)));
    }
}
