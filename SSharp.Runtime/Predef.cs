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

    public static SSharpList<T> List<T>(params T[] items)
    {
        SSharpList<T> list = new Nil<T>();
        for (int i = items.Length - 1; i >= 0; i--)
        {
            list = new Cons<T>(items[i], list);
        }
        return list;
    }
}
