using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace SSharp.Runtime;

/// <summary>
/// Immutable Set, equivalent to Scala's immutable Set.
/// All operations return new instances, the original is never mutated.
/// </summary>
public sealed class SSharpSet<T>
{
    private readonly ImmutableHashSet<T> _inner;

    private SSharpSet(ImmutableHashSet<T> inner)
    {
        _inner = inner;
    }

    internal SSharpSet(IEnumerable<T> items)
    {
        _inner = ImmutableHashSet.CreateRange(items);
    }

    internal ImmutableHashSet<T> Inner => _inner;

    // ── Queries ────────────────────────────────────────────────────────────────

    public bool IsEmpty => _inner.IsEmpty;

    public int Size => _inner.Count;

    public bool Contains(T elem) => _inner.Contains(elem);

    // ── Transformations (return new set) ──────────────────────────────────────

    /// <summary>Returns a new Set with elem added.</summary>
    public SSharpSet<T> Incl(T elem) => new(_inner.Add(elem));

    /// <summary>Returns a new Set with elem removed.</summary>
    public SSharpSet<T> Excl(T elem) => new(_inner.Remove(elem));

    /// <summary>Returns the union of this set and other.</summary>
    public SSharpSet<T> Union(SSharpSet<T> other) => new(_inner.Union(other._inner));

    /// <summary>Returns the intersection of this set and other.</summary>
    public SSharpSet<T> Intersect(SSharpSet<T> other) => new(_inner.Intersect(other._inner));

    /// <summary>Returns the difference of this set minus other.</summary>
    public SSharpSet<T> Diff(SSharpSet<T> other) => new(_inner.Except(other._inner));

    /// <summary>Applies f to every element and returns a new Set of results.</summary>
    public SSharpSet<U> Map<U>(Func<T, U> f) => new SSharpSet<U>(_inner.Select(f));

    /// <summary>Returns a new Set with only the elements that satisfy p.</summary>
    public SSharpSet<T> Filter(Func<T, bool> p) => new(_inner.Where(p));

    public SSharpSet<U> FlatMap<U>(Func<T, SSharpSet<U>> f)
    {
        var list = new List<U>();
        foreach (var elem in _inner)
        {
            list.AddRange(f(elem).Inner);
        }
        return new SSharpSet<U>(list);
    }

    public SSharpSet<U> Flatten<U>()
    {
        var list = new List<U>();
        foreach (var elem in _inner)
        {
            if (elem is SSharpSet<U> innerSet)
            {
                list.AddRange(innerSet.Inner);
            }
        }
        return new SSharpSet<U>(list);
    }

    // ── Fold / iteration ──────────────────────────────────────────────────────

    public U FoldLeft<U>(U z, Func<U, T, U> f)
    {
        var acc = z;
        foreach (var elem in _inner)
        {
            acc = f(acc, elem);
        }
        return acc;
    }

    public void Foreach(Action<T> action)
    {
        foreach (var elem in _inner)
        {
            action(elem);
        }
    }

    // ── Conversions ───────────────────────────────────────────────────────────

    public SSharpList<T> ToList()
    {
        SSharpList<T> list = new Nil<T>();
        foreach (var elem in _inner.Reverse())
        {
            list = new Cons<T>(elem, list);
        }
        return list;
    }

    // ── Display ───────────────────────────────────────────────────────────────

    public override string ToString()
    {
        var items = _inner.Select(x => x?.ToString() ?? "null");
        return $"Set({string.Join(", ", items)})";
    }

    public override bool Equals(object? obj)
    {
        if (obj is SSharpSet<T> other)
        {
            return _inner.SetEquals(other._inner);
        }
        return false;
    }

    public override int GetHashCode() => _inner.GetHashCode();
}
