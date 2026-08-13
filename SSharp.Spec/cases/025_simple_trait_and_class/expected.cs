using System;

public interface Person {}
public record Student(string name, int age) : Person;

public static class Program
{
    public static void Main()
    {
        Student s = new Student("Alice", 20);
        Console.WriteLine(s.name);
        Console.WriteLine(s.age);
    }
}
