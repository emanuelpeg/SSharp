using System;
using System.Collections.Generic;

namespace SSharp.Runtime;

public abstract record SSharpList<T>
{
    public abstract bool IsEmpty { get; }
    public abstract int Length { get; }

    /// <summary>Alias for Length (Scala-style).</summary>
    public int Size => Length;

    // ── Core HOFs ─────────────────────────────────────────────────────────────

    public SSharpList<U> Map<U>(Func<T, U> f)
    {
        if (this is Cons<T> cons)
        {
            return new Cons<U>(f(cons.Head), cons.Tail.Map(f));
        }
        return new Nil<U>();
    }

    public SSharpList<T> Filter(Func<T, bool> p)
    {
        if (this is Cons<T> cons)
        {
            if (p(cons.Head))
            {
                return new Cons<T>(cons.Head, cons.Tail.Filter(p));
            }
            return cons.Tail.Filter(p);
        }
        return new Nil<T>();
    }

    public SSharpList<U> FlatMap<U>(Func<T, SSharpList<U>> f)
    {
        SSharpList<U> result = new Nil<U>();
        var curr = this;
        // Collect reversed segments then concat
        var segments = new List<SSharpList<U>>();
        while (curr is Cons<T> cons)
        {
            segments.Add(f(cons.Head));
            curr = cons.Tail;
        }
        for (int i = segments.Count - 1; i >= 0; i--)
        {
            result = segments[i].Concat(result);
        }
        return result;
    }

    public U FoldLeft<U>(U z, Func<U, T, U> f)
    {
        var curr = this;
        var acc = z;
        while (curr is Cons<T> cons)
        {
            acc = f(acc, cons.Head);
            curr = cons.Tail;
        }
        return acc;
    }

    public void Foreach(Action<T> action)
    {
        var curr = this;
        while (curr is Cons<T> cons)
        {
            action(cons.Head);
            curr = cons.Tail;
        }
    }

    // ── Element access ────────────────────────────────────────────────────────

    public T HeadValue =>
        this is Cons<T> c ? c.Head : throw new InvalidOperationException("head of empty list");

    public SSharpList<T> TailList =>
        this is Cons<T> c ? c.Tail : throw new InvalidOperationException("tail of empty list");

    public bool Contains(T elem)
    {
        var curr = this;
        while (curr is Cons<T> cons)
        {
            if (Equals(cons.Head, elem)) return true;
            curr = cons.Tail;
        }
        return false;
    }

    public SSharpOption<T> Find(Func<T, bool> p)
    {
        var curr = this;
        while (curr is Cons<T> cons)
        {
            if (p(cons.Head)) return new Some<T>(cons.Head);
            curr = cons.Tail;
        }
        return new None<T>();
    }

    public bool Forall(Func<T, bool> p)
    {
        var curr = this;
        while (curr is Cons<T> cons)
        {
            if (!p(cons.Head)) return false;
            curr = cons.Tail;
        }
        return true;
    }

    public bool Exists(Func<T, bool> p)
    {
        var curr = this;
        while (curr is Cons<T> cons)
        {
            if (p(cons.Head)) return true;
            curr = cons.Tail;
        }
        return false;
    }

    // ── Structural operations (return new list) ───────────────────────────────

    /// <summary>Returns a new list with elem appended at the end (:+).</summary>
    public SSharpList<T> Appended(T elem)
    {
        // Build by reversing, prepending, then reversing back
        return Reverse().Prepended(elem).Reverse();
    }

    /// <summary>Returns a new list with elem prepended at the front (::).</summary>
    public SSharpList<T> Prepended(T elem) => new Cons<T>(elem, this);

    /// <summary>Concatenates this list with other (++).</summary>
    public SSharpList<T> Concat(SSharpList<T> other)
    {
        if (this is Cons<T> cons)
        {
            return new Cons<T>(cons.Head, cons.Tail.Concat(other));
        }
        return other;
    }

    /// <summary>Returns the first n elements.</summary>
    public SSharpList<T> Take(int n)
    {
        if (n <= 0 || this is not Cons<T> cons) return new Nil<T>();
        return new Cons<T>(cons.Head, cons.Tail.Take(n - 1));
    }

    /// <summary>Returns the list without the first n elements.</summary>
    public SSharpList<T> Drop(int n)
    {
        var curr = this;
        while (n > 0 && curr is Cons<T> cons)
        {
            curr = cons.Tail;
            n--;
        }
        return curr;
    }

    /// <summary>Returns the list in reversed order.</summary>
    public SSharpList<T> Reverse()
    {
        SSharpList<T> acc = new Nil<T>();
        var curr = this;
        while (curr is Cons<T> cons)
        {
            acc = new Cons<T>(cons.Head, acc);
            curr = cons.Tail;
        }
        return acc;
    }

    // ── Conversions ───────────────────────────────────────────────────────────

    /// <summary>Converts this list to an immutable Set.</summary>
    public SSharpSet<T> ToSet()
    {
        var items = new List<T>();
        var curr = this;
        while (curr is Cons<T> cons)
        {
            items.Add(cons.Head);
            curr = cons.Tail;
        }
        return new SSharpSet<T>(items);
    }

    public SSharpList<U> Flatten<U>()
    {
        SSharpList<U> result = new Nil<U>();
        var curr = this;
        var segments = new List<SSharpList<U>>();
        while (curr is Cons<T> cons)
        {
            if (cons.Head is SSharpList<U> inner)
            {
                segments.Add(inner);
            }
            curr = cons.Tail;
        }
        for (int i = segments.Count - 1; i >= 0; i--)
        {
            result = segments[i].Concat(result);
        }
        return result;
    }


    // ── Display ───────────────────────────────────────────────────────────────

    public override string ToString()
    {
        var items = new List<string>();
        var curr = this;
        while (curr is Cons<T> cons)
        {
            items.Add(cons.Head?.ToString() ?? "null");
            curr = cons.Tail;
        }
        return $"List({string.Join(", ", items)})";
    }
}

public record Nil<T> : SSharpList<T>
{
    public override bool IsEmpty => true;
    public override int Length => 0;
    public override string ToString() => base.ToString();
}

public record Cons<T>(T Head, SSharpList<T> Tail) : SSharpList<T>
{
    public override bool IsEmpty => false;
    public override int Length => 1 + Tail.Length;
    public override string ToString() => base.ToString();
}
