using System;
using System.Collections.Generic;

namespace SSharp.Runtime;

public abstract record SSharpList<T>
{
    public abstract bool IsEmpty { get; }
    public abstract int Length { get; }

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
}

public record Cons<T>(T Head, SSharpList<T> Tail) : SSharpList<T>
{
    public override bool IsEmpty => false;
    public override int Length => 1 + Tail.Length;
}
