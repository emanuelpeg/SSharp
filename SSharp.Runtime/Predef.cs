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

    // -- List factory ---------------------------------------------------------

    public static SSharpList<T> List<T>(params T[] items)
    {
        SSharpList<T> list = new Nil<T>();
        for (int i = items.Length - 1; i >= 0; i--)
        {
            list = new Cons<T>(items[i], list);
        }
        return list;
    }

    // -- List free functions -- Inspection ------------------------------------

    /// <summary>Returns the number of elements in the list.</summary>
    public static int length<T>(SSharpList<T> xs) => xs.Length;

    /// <summary>Alias for length.</summary>
    public static int size<T>(SSharpList<T> xs) => xs.Length;

    /// <summary>Returns true if the list has no elements.</summary>
    public static bool isEmpty<T>(SSharpList<T> xs) => xs.IsEmpty;

    /// <summary>Returns the first element of the list.</summary>
    public static T head<T>(SSharpList<T> xs) => xs.HeadValue;

    /// <summary>Returns the rest of the list after the head.</summary>
    public static SSharpList<T> tail<T>(SSharpList<T> xs) => xs.TailList;

    /// <summary>Returns true if elem is in the list.</summary>
    public static bool contains<T>(T elem, SSharpList<T> xs) => xs.Contains(elem);

    // -- List free functions -- Transformation --------------------------------

    /// <summary>Applies f to each element and returns a new list of results.</summary>
    public static SSharpList<U> map<T, U>(Func<T, U> f, SSharpList<T> xs) => xs.Map(f);

    /// <summary>Returns a new list of elements that satisfy predicate p.</summary>
    public static SSharpList<T> filter<T>(Func<T, bool> p, SSharpList<T> xs) => xs.Filter(p);

    /// <summary>Applies f to each element and flattens the resulting lists.</summary>
    public static SSharpList<U> flatMap<T, U>(Func<T, SSharpList<U>> f, SSharpList<T> xs) => xs.FlatMap(f);

    /// <summary>Returns the list in reversed order.</summary>
    public static SSharpList<T> reverse<T>(SSharpList<T> xs) => xs.Reverse();

    /// <summary>Concatenates two lists.</summary>
    public static SSharpList<T> concat<T>(SSharpList<T> xs, SSharpList<T> ys) => xs.Concat(ys);

    // -- List free functions -- Reduction -------------------------------------

    /// <summary>Left fold: reduces the list from left to right starting with accumulator z.</summary>
    public static U foldLeft<T, U>(U z, Func<U, T, U> f, SSharpList<T> xs) => xs.FoldLeft(z, f);

    /// <summary>Right fold: reduces the list from right to left starting with accumulator z.</summary>
    public static U foldRight<T, U>(U z, Func<T, U, U> f, SSharpList<T> xs)
    {
        return xs.Reverse().FoldLeft(z, (acc, x) => f(x, acc));
    }

    /// <summary>Sums the integer elements of a list.</summary>
    public static int sum(SSharpList<int> xs) => xs.FoldLeft(0, (acc, x) => acc + x);

    /// <summary>Sums the double elements of a list.</summary>
    public static double sumDouble(SSharpList<double> xs) => xs.FoldLeft(0.0, (acc, x) => acc + x);

    /// <summary>Returns the product of the integer elements of a list.</summary>
    public static int product(SSharpList<int> xs) => xs.FoldLeft(1, (acc, x) => acc * x);

    /// <summary>Returns the product of the double elements of a list.</summary>
    public static double productDouble(SSharpList<double> xs) => xs.FoldLeft(1.0, (acc, x) => acc * x);

    /// <summary>Reduces a non-empty list using a binary operator (left-associative).</summary>
    public static T reduce<T>(Func<T, T, T> f, SSharpList<T> xs)
    {
        if (xs.IsEmpty) throw new InvalidOperationException("reduce of empty list");
        return xs.TailList.FoldLeft(xs.HeadValue, f);
    }

    // -- List free functions -- Subsets ----------------------------------------

    /// <summary>Returns the first n elements of the list.</summary>
    public static SSharpList<T> take<T>(int n, SSharpList<T> xs) => xs.Take(n);

    /// <summary>Drops the first n elements and returns the rest.</summary>
    public static SSharpList<T> drop<T>(int n, SSharpList<T> xs) => xs.Drop(n);

    /// <summary>Returns elements while predicate p holds.</summary>
    public static SSharpList<T> takeWhile<T>(Func<T, bool> p, SSharpList<T> xs)
    {
        var buf = new System.Collections.Generic.List<T>();
        var curr = xs;
        while (curr is Cons<T> cons && p(cons.Head))
        {
            buf.Add(cons.Head);
            curr = cons.Tail;
        }
        SSharpList<T> acc = new Nil<T>();
        for (int i = buf.Count - 1; i >= 0; i--)
            acc = new Cons<T>(buf[i], acc);
        return acc;
    }

    /// <summary>Drops elements while predicate p holds, then returns the rest.</summary>
    public static SSharpList<T> dropWhile<T>(Func<T, bool> p, SSharpList<T> xs)
    {
        var curr = xs;
        while (curr is Cons<T> cons && p(cons.Head))
            curr = cons.Tail;
        return curr;
    }

    // -- List free functions -- Combination -----------------------------------

    /// <summary>Pairs corresponding elements from two lists into Tuple2s.</summary>
    public static SSharpList<SSharpTuple2<A, B>> zip<A, B>(SSharpList<A> xs, SSharpList<B> ys)
    {
        var buf = new System.Collections.Generic.List<SSharpTuple2<A, B>>();
        var cx = xs;
        var cy = ys;
        while (cx is Cons<A> cx2 && cy is Cons<B> cy2)
        {
            buf.Add(new SSharpTuple2<A, B>(cx2.Head, cy2.Head));
            cx = cx2.Tail;
            cy = cy2.Tail;
        }
        SSharpList<SSharpTuple2<A, B>> acc = new Nil<SSharpTuple2<A, B>>();
        for (int i = buf.Count - 1; i >= 0; i--)
            acc = new Cons<SSharpTuple2<A, B>>(buf[i], acc);
        return acc;
    }

    /// <summary>Applies a binary function to corresponding elements of two lists.</summary>
    public static SSharpList<C> zipWith<A, B, C>(Func<A, B, C> f, SSharpList<A> xs, SSharpList<B> ys)
    {
        var buf = new System.Collections.Generic.List<C>();
        var cx = xs;
        var cy = ys;
        while (cx is Cons<A> cx2 && cy is Cons<B> cy2)
        {
            buf.Add(f(cx2.Head, cy2.Head));
            cx = cx2.Tail;
            cy = cy2.Tail;
        }
        SSharpList<C> acc = new Nil<C>();
        for (int i = buf.Count - 1; i >= 0; i--)
            acc = new Cons<C>(buf[i], acc);
        return acc;
    }

    // -- Set factory ----------------------------------------------------------

    /// <summary>Creates an immutable Set from the given elements.</summary>
    public static SSharpSet<T> Set<T>(params T[] items) => new SSharpSet<T>(items);

    // -- Map factory ----------------------------------------------------------

    /// <summary>Creates an immutable Map from the given key-value Tuple2 pairs.</summary>
    public static SSharpMap<K, V> Map<K, V>(params SSharpTuple2<K, V>[] entries) =>
        new SSharpMap<K, V>(entries);

    // -- Tuple2 factory -------------------------------------------------------

    /// <summary>Creates a Tuple2 pair. Used as map entries: Tuple2("key", value).</summary>
    public static SSharpTuple2<A, B> Tuple2<A, B>(A a, B b) => new SSharpTuple2<A, B>(a, b);

    // -- Option factory -------------------------------------------------------

    /// <summary>Creates a Some option.</summary>
    public static Some<T> Some<T>(T value) => new Some<T>(value);

    /// <summary>Creates an Option from the given value (Some if not null, None otherwise).</summary>
    public static SSharpOption<T> Option<T>(T value) =>
        value == null ? new None<T>() : new Some<T>(value);

    // -- Cons factory ---------------------------------------------------------

    /// <summary>Creates a Cons cell for list.</summary>
    public static Cons<T> Cons<T>(T head, SSharpList<T> tail) => new Cons<T>(head, tail);
}
