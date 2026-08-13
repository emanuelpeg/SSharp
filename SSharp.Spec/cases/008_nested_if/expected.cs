using System;

public static class Program
{
    public static void Main()
    {
        int score = 85;
        string grade = score >= 90 ? "A" : score >= 80 ? "B" : "C";
        Console.WriteLine(grade);
    }
}
