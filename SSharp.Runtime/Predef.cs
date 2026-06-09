using System;

namespace SSharp.Runtime;

public static class Predef
{
    public static Unit print(object? x)
    {
        Console.Write(x);
        return Unit.Instance;
    }

    public static Unit println(object? x)
    {
        Console.WriteLine(x);
        return Unit.Instance;
    }

    public static string readLine()
    {
        return Console.ReadLine() ?? "";
    }
}
