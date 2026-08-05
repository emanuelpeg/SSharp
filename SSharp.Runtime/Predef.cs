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

    // ── List factory ─────────────────────────────────────────────────────────

    public static SSharpList<T> List<T>(params T[] items)
    {
        SSharpList<T> list = new Nil<T>();
        for (int i = items.Length - 1; i >= 0; i--)
        {
            list = new Cons<T>(items[i], list);
        }
        return list;
    }

    // ── List operations ───────────────────────────────────────────────────────

    /// <summary>Returns the first element of the list (head).</summary>
    public static T head<T>(SSharpList<T> list) => list.HeadValue;

    /// <summary>Returns the rest of the list (tail).</summary>
    public static SSharpList<T> tail<T>(SSharpList<T> list) => list.TailList;

    // ── Set factory ──────────────────────────────────────────────────────────

    /// <summary>Creates an immutable Set from the given elements.</summary>
    public static SSharpSet<T> Set<T>(params T[] items) => new SSharpSet<T>(items);

    // ── Map factory ──────────────────────────────────────────────────────────

    /// <summary>Creates an immutable Map from the given key-value Tuple2 pairs.</summary>
    public static SSharpMap<K, V> Map<K, V>(params SSharpTuple2<K, V>[] entries) =>
        new SSharpMap<K, V>(entries);

    // ── Tuple2 factory ───────────────────────────────────────────────────────

    /// <summary>Creates a Tuple2 pair. Used as map entries: Tuple2("key", value).</summary>
    public static SSharpTuple2<A, B> Tuple2<A, B>(A a, B b) => new SSharpTuple2<A, B>(a, b);

    // ── Option factory ───────────────────────────────────────────────────────

    /// <summary>Creates an Option from the given value (Some if not null, None otherwise).</summary>
    public static SSharpOption<T> Option<T>(T value) =>
        value == null ? new None<T>() : new Some<T>(value);
}
