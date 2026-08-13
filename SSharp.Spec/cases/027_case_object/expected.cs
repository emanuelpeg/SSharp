using System;

public interface Color {}
public record Red : Color {
    private Red() {}
    public static Red Instance { get; } = new Red();
}

public static class Program
{
    public static void Main()
    {
        Red c = Red.Instance;
        Console.WriteLine(c != null);
    }
}
