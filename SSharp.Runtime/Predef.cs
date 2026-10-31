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

    /// <summary>Returns the first element of the list. Throws on empty list. Prefer headOption(xs).</summary>
    public static T head<T>(SSharpList<T> xs) =>
        xs is Cons<T> c ? c.Head : throw new InvalidOperationException("head of empty list");

    /// <summary>Returns the rest of the list after the head. Throws on empty list. Prefer tailOption(xs).</summary>
    public static SSharpList<T> tail<T>(SSharpList<T> xs) =>
        xs is Cons<T> c ? c.Tail : throw new InvalidOperationException("tail of empty list");

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
        if (xs is not Cons<T> first) throw new InvalidOperationException("reduce of empty list");
        return first.Tail.FoldLeft(first.Head, f);
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

    // -- Tuple factories ------------------------------------------------------

    /// <summary>Creates a Tuple2 pair.</summary>
    public static SSharpTuple2<A, B> Tuple2<A, B>(A a, B b) => new SSharpTuple2<A, B>(a, b);

    /// <summary>Creates a Tuple3 triplet.</summary>
    public static SSharpTuple3<A, B, C> Tuple3<A, B, C>(A a, B b, C c) => new SSharpTuple3<A, B, C>(a, b, c);

    /// <summary>Creates a Tuple4 quadruplet.</summary>
    public static SSharpTuple4<A, B, C, D> Tuple4<A, B, C, D>(A a, B b, C c, D d) => new SSharpTuple4<A, B, C, D>(a, b, c, d);

    /// <summary>Creates a Tuple5 quintuplet.</summary>
    public static SSharpTuple5<A, B, C, D, E> Tuple5<A, B, C, D, E>(A a, B b, C c, D d, E e) => new SSharpTuple5<A, B, C, D, E>(a, b, c, d, e);

    /// <summary>Creates a Tuple6 sextuplet.</summary>
    public static SSharpTuple6<A, B, C, D, E, F> Tuple6<A, B, C, D, E, F>(A a, B b, C c, D d, E e, F f) => new SSharpTuple6<A, B, C, D, E, F>(a, b, c, d, e, f);

    /// <summary>Creates a Tuple7 septuplet.</summary>
    public static SSharpTuple7<A, B, C, D, E, F, G> Tuple7<A, B, C, D, E, F, G>(A a, B b, C c, D d, E e, F f, G g) => new SSharpTuple7<A, B, C, D, E, F, G>(a, b, c, d, e, f, g);

    /// <summary>Creates a Tuple8 octuplet.</summary>
    public static SSharpTuple8<A, B, C, D, E, F, G, H> Tuple8<A, B, C, D, E, F, G, H>(A a, B b, C c, D d, E e, F f, G g, H h) => new SSharpTuple8<A, B, C, D, E, F, G, H>(a, b, c, d, e, f, g, h);

    // -- Option factory -------------------------------------------------------

    /// <summary>Creates a Some option.</summary>
    public static Some<T> Some<T>(T value) => new Some<T>(value);

    /// <summary>Creates an Option from the given value (Some if not null, None otherwise).</summary>
    public static SSharpOption<T> Option<T>(T value) =>
        value == null ? new None<T>() : new Some<T>(value);

    // -- Option free functions ------------------------------------------------

    /// <summary>Returns true if the option has no value.</summary>
    public static bool isEmpty<T>(SSharpOption<T> opt) => !opt.IsDefined;

    /// <summary>Returns true if the option has a value.</summary>
    public static bool isDefined<T>(SSharpOption<T> opt) => opt.IsDefined;

    /// <summary>Alias for isDefined.</summary>
    public static bool isSome<T>(SSharpOption<T> opt) => opt.IsDefined;

    /// <summary>Alias for isEmpty.</summary>
    public static bool isNone<T>(SSharpOption<T> opt) => !opt.IsDefined;

    /// <summary>Returns the value inside opt if defined, or defaultVal otherwise.</summary>
    public static T getOrElse<T>(T defaultVal, SSharpOption<T> opt) => opt.GetOrElse(() => defaultVal);

    /// <summary>Returns the value inside opt if defined, or invokes defaultSupplier.</summary>
    public static T getOrElse<T>(Func<T> defaultSupplier, SSharpOption<T> opt) => opt.GetOrElse(defaultSupplier);

    /// <summary>Applies f to the value inside opt if defined, returning a new Option.</summary>
    public static SSharpOption<U> map<T, U>(Func<T, U> f, SSharpOption<T> opt) => opt.Map(f);

    /// <summary>Returns opt if defined and predicate p holds, None otherwise.</summary>
    public static SSharpOption<T> filter<T>(Func<T, bool> p, SSharpOption<T> opt) => opt.Filter(p);

    /// <summary>Applies f returning an Option if opt is defined, None otherwise.</summary>
    public static SSharpOption<U> flatMap<T, U>(Func<T, SSharpOption<U>> f, SSharpOption<T> opt) => opt.FlatMap(f);

    /// <summary>Flattens a nested Option (Option[Option[T]] -> Option[T]).</summary>
    public static SSharpOption<T> flatten<T>(SSharpOption<SSharpOption<T>> opt) => opt.Flatten<T>();

    // -- List extended free functions -----------------------------------------

    /// <summary>Returns Some(head) if non-empty, None otherwise (Haskell: listToMaybe).</summary>
    public static SSharpOption<T> headOption<T>(SSharpList<T> xs) =>
        xs is Cons<T> c ? new Some<T>(c.Head) : new None<T>();

    /// <summary>Returns Some(tail) if non-empty, None otherwise.</summary>
    public static SSharpOption<SSharpList<T>> tailOption<T>(SSharpList<T> xs) =>
        xs is Cons<T> c ? new Some<SSharpList<T>>(c.Tail) : new None<SSharpList<T>>();

    /// <summary>Deconstructs a list into head and tail safely (Haskell: uncons).</summary>
    public static SSharpOption<SSharpTuple2<T, SSharpList<T>>> uncons<T>(SSharpList<T> xs) =>
        xs is Cons<T> c ? new Some<SSharpTuple2<T, SSharpList<T>>>(new SSharpTuple2<T, SSharpList<T>>(c.Head, c.Tail)) : new None<SSharpTuple2<T, SSharpList<T>>>();

    /// <summary>Returns all elements except the last one (Haskell: init). Throws on empty list.</summary>
    public static SSharpList<T> init<T>(SSharpList<T> xs)
    {
        if (xs is not Cons<T> c) throw new InvalidOperationException("init of empty list");
        if (c.Tail.IsEmpty) return new Nil<T>();
        return new Cons<T>(c.Head, init(c.Tail));
    }

    /// <summary>Returns the last element of the list (Haskell: last). Throws on empty list.</summary>
    public static T last<T>(SSharpList<T> xs)
    {
        if (xs is not Cons<T> first) throw new InvalidOperationException("last of empty list");
        var curr = first;
        while (curr.Tail is Cons<T> next)
        {
            curr = next;
        }
        return curr.Head;
    }

    /// <summary>Returns true if all elements satisfy predicate p (Haskell: all).</summary>
    public static bool forall<T>(Func<T, bool> p, SSharpList<T> xs) => xs.Forall(p);

    /// <summary>Returns true if any element satisfies predicate p (Haskell: any).</summary>
    public static bool exists<T>(Func<T, bool> p, SSharpList<T> xs) => xs.Exists(p);

    /// <summary>Appends an element to the end of the list.</summary>
    public static SSharpList<T> append<T>(T elem, SSharpList<T> xs) => xs.Appended(elem);
    public static SSharpList<T> append<T>(SSharpList<T> xs, T elem) => xs.Appended(elem);
    public static SSharpList<T> appended<T>(T elem, SSharpList<T> xs) => xs.Appended(elem);
    public static SSharpList<T> appended<T>(SSharpList<T> xs, T elem) => xs.Appended(elem);

    /// <summary>Prepends an element to the front of the list.</summary>
    public static SSharpList<T> prepend<T>(T elem, SSharpList<T> xs) => xs.Prepended(elem);
    public static SSharpList<T> prepended<T>(T elem, SSharpList<T> xs) => xs.Prepended(elem);

    /// <summary>Flattens a list of lists into a single list (Haskell: concat).</summary>
    public static SSharpList<T> flatten<T>(SSharpList<SSharpList<T>> xss) => xss.Flatten<T>();

    // -- Set free functions ---------------------------------------------------

    /// <summary>Returns the number of elements in the set.</summary>
    public static int size<T>(SSharpSet<T> s) => s.Size;

    /// <summary>Returns true if the set is empty.</summary>
    public static bool isEmpty<T>(SSharpSet<T> s) => s.IsEmpty;

    /// <summary>Returns true if elem is in the set (Haskell: member).</summary>
    public static bool contains<T>(T elem, SSharpSet<T> s) => s.Contains(elem);

    /// <summary>Returns a new set with elem added (Haskell: insert).</summary>
    public static SSharpSet<T> insert<T>(T elem, SSharpSet<T> s) => s.Incl(elem);
    public static SSharpSet<T> incl<T>(T elem, SSharpSet<T> s) => s.Incl(elem);

    /// <summary>Returns a new set with elem removed (Haskell: delete).</summary>
    public static SSharpSet<T> delete<T>(T elem, SSharpSet<T> s) => s.Excl(elem);
    public static SSharpSet<T> excl<T>(T elem, SSharpSet<T> s) => s.Excl(elem);

    /// <summary>Returns the union of two sets.</summary>
    public static SSharpSet<T> union<T>(SSharpSet<T> a, SSharpSet<T> b) => a.Union(b);

    /// <summary>Returns the intersection of two sets.</summary>
    public static SSharpSet<T> intersect<T>(SSharpSet<T> a, SSharpSet<T> b) => a.Intersect(b);

    /// <summary>Returns the difference of set a minus set b.</summary>
    public static SSharpSet<T> diff<T>(SSharpSet<T> a, SSharpSet<T> b) => a.Diff(b);

    /// <summary>Converts the set to an immutable list.</summary>
    public static SSharpList<T> toList<T>(SSharpSet<T> s) => s.ToList();

    /// <summary>Applies f to each element in the set.</summary>
    public static SSharpSet<U> map<T, U>(Func<T, U> f, SSharpSet<T> s) => s.Map(f);

    /// <summary>Filters the set by predicate p.</summary>
    public static SSharpSet<T> filter<T>(Func<T, bool> p, SSharpSet<T> s) => s.Filter(p);

    /// <summary>Flattens a nested set of sets.</summary>
    public static SSharpSet<T> flatten<T>(SSharpSet<SSharpSet<T>> s) => s.Flatten<T>();

    /// <summary>Applies f returning sets and flattens the result.</summary>
    public static SSharpSet<U> flatMap<T, U>(Func<T, SSharpSet<U>> f, SSharpSet<T> s) => s.FlatMap(f);

    // -- Map free functions ---------------------------------------------------

    /// <summary>Returns the number of entries in the map.</summary>
    public static int size<K, V>(SSharpMap<K, V> m) => m.Size;

    /// <summary>Returns true if the map is empty.</summary>
    public static bool isEmpty<K, V>(SSharpMap<K, V> m) => m.IsEmpty;

    /// <summary>Returns true if the key exists in the map (Haskell: member).</summary>
    public static bool contains<K, V>(K k, SSharpMap<K, V> m) => m.Contains(k);

    /// <summary>Returns Some(value) if key exists in the map, None otherwise (Haskell: lookup).</summary>
    public static SSharpOption<V> get<K, V>(K k, SSharpMap<K, V> m) => m.Get(k);

    /// <summary>Returns a new map with the key-value pair updated or inserted.</summary>
    public static SSharpMap<K, V> insert<K, V>(K k, V v, SSharpMap<K, V> m) => m.Updated(k, v);
    public static SSharpMap<K, V> updated<K, V>(K k, V v, SSharpMap<K, V> m) => m.Updated(k, v);

    /// <summary>Returns a new map with the key removed.</summary>
    public static SSharpMap<K, V> delete<K, V>(K k, SSharpMap<K, V> m) => m.Removed(k);
    public static SSharpMap<K, V> removed<K, V>(K k, SSharpMap<K, V> m) => m.Removed(k);

    /// <summary>Returns a set containing all keys of the map.</summary>
    public static SSharpSet<K> keys<K, V>(SSharpMap<K, V> m) => m.Keys();

    /// <summary>Returns a list containing all values of the map.</summary>
    public static SSharpList<V> values<K, V>(SSharpMap<K, V> m) => m.Values();

    /// <summary>Converts the map to a list of key-value pairs (Haskell: toList).</summary>
    public static SSharpList<SSharpTuple2<K, V>> toList<K, V>(SSharpMap<K, V> m) => m.ToList();

    /// <summary>Applies f returning maps and flattens the result.</summary>
    public static SSharpMap<K2, V2> flatMap<K, V, K2, V2>(Func<SSharpTuple2<K, V>, SSharpMap<K2, V2>> f, SSharpMap<K, V> m) => m.FlatMap(f);

    // -- Tuple free functions -------------------------------------------------

    /// <summary>Returns the first element of a pair (Haskell: fst).</summary>
    public static A fst<A, B>(SSharpTuple2<A, B> t) => t._1;

    /// <summary>Returns the second element of a pair (Haskell: snd).</summary>
    public static B snd<A, B>(SSharpTuple2<A, B> t) => t._2;

    // -- Pure functional combinators ------------------------------------------

    /// <summary>Identity function (Haskell: id).</summary>
    public static T id<T>(T x) => x;

    /// <summary>Constant function (Haskell: const).</summary>
    public static A constVal<A, B>(A a, B b) => a;

    // -- Cons factory ---------------------------------------------------------

    /// <summary>Creates a Cons cell for list.</summary>
    public static Cons<T> Cons<T>(T head, SSharpList<T> tail) => new Cons<T>(head, tail);
}
