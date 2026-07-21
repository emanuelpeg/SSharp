using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SSharp.Runtime;

/// <summary>
/// Immutable Map, equivalent to Scala's immutable Map.
/// All operations return new instances, the original is never mutated.
/// Entries are created with Tuple2: Map(("key", value), ...)
/// </summary>
#pragma warning disable CS8714 // K may not be notnull, but SSharp supports any reference type as key
public sealed class SSharpMap<K, V>
{
    private readonly ImmutableDictionary<K, V> _inner;

    internal SSharpMap(ImmutableDictionary<K, V> inner)
    {
        _inner = inner;
    }

    internal SSharpMap(IEnumerable<SSharpTuple2<K, V>> entries)
    {
        _inner = entries.ToImmutableDictionary(e => e._1, e => e._2);
    }

    internal ImmutableDictionary<K, V> Inner => _inner;
#pragma warning restore CS8714

    // ── Queries ────────────────────────────────────────────────────────────────

    public bool IsEmpty => _inner.IsEmpty;

    public int Size => _inner.Count;

    public bool Contains(K key) => _inner.ContainsKey(key!);

    /// <summary>Returns Some(value) if key exists, None otherwise.</summary>
    public SSharpOption<V> Get(K key)
    {
        if (_inner.TryGetValue(key!, out V? value))
        {
            return new Some<V>(value);
        }
        return new None<V>();
    }

    /// <summary>Returns the value for key; throws if key is not present.</summary>
    public V Apply(K key)
    {
        if (_inner.TryGetValue(key!, out V? value))
        {
            return value;
        }
        throw new KeyNotFoundException($"Key not found in Map: {key}");
    }

    // ── Transformations (return new map) ──────────────────────────────────────

    /// <summary>Returns a new Map with the key-value pair added/updated.</summary>
    public SSharpMap<K, V> Updated(K key, V value) => new(_inner.SetItem(key!, value));

    /// <summary>Returns a new Map with the key removed.</summary>
    public SSharpMap<K, V> Removed(K key) => new(_inner.Remove(key!));

    /// <summary>Applies f to every (key, value) pair and returns a new Map.</summary>
    public SSharpMap<K2, V2> Map<K2, V2>(Func<SSharpTuple2<K, V>, SSharpTuple2<K2, V2>> f)
    {
        var entries = _inner.Select(kv => f(new SSharpTuple2<K, V>(kv.Key, kv.Value)));
        return new SSharpMap<K2, V2>(entries);
    }

    /// <summary>Returns a new Map with only the entries that satisfy p.</summary>
    public SSharpMap<K, V> Filter(Func<SSharpTuple2<K, V>, bool> p)
    {
#pragma warning disable CS8714
        var builder = ImmutableDictionary.CreateBuilder<K, V>();
#pragma warning restore CS8714
        foreach (var kv in _inner)
        {
            if (p(new SSharpTuple2<K, V>(kv.Key, kv.Value)))
            {
                builder.Add(kv.Key, kv.Value);
            }
        }
        return new SSharpMap<K, V>(builder.ToImmutable());
    }

    public SSharpMap<K2, V2> FlatMap<K2, V2>(Func<SSharpTuple2<K, V>, SSharpMap<K2, V2>> f)
    {
#pragma warning disable CS8714
        var builder = ImmutableDictionary.CreateBuilder<K2, V2>();
#pragma warning restore CS8714
        foreach (var kv in _inner)
        {
            var mappedMap = f(new SSharpTuple2<K, V>(kv.Key, kv.Value));
            foreach (var mappedKv in mappedMap.Inner)
            {
                builder[mappedKv.Key] = mappedKv.Value;
            }
        }
        return new SSharpMap<K2, V2>(builder.ToImmutable());
    }

    // ── Views ─────────────────────────────────────────────────────────────────

    public SSharpSet<K> Keys() => new SSharpSet<K>(_inner.Keys);

    public SSharpList<V> Values()
    {
        SSharpList<V> list = new Nil<V>();
        foreach (var v in _inner.Values.Reverse())
        {
            list = new Cons<V>(v, list);
        }
        return list;
    }

    // ── Fold / iteration ──────────────────────────────────────────────────────

    public U FoldLeft<U>(U z, Func<U, SSharpTuple2<K, V>, U> f)
    {
        var acc = z;
        foreach (var kv in _inner)
        {
            acc = f(acc, new SSharpTuple2<K, V>(kv.Key, kv.Value));
        }
        return acc;
    }

    public void Foreach(Action<SSharpTuple2<K, V>> action)
    {
        foreach (var kv in _inner)
        {
            action(new SSharpTuple2<K, V>(kv.Key, kv.Value));
        }
    }

    // ── Conversions ───────────────────────────────────────────────────────────

    public SSharpList<SSharpTuple2<K, V>> ToList()
    {
        SSharpList<SSharpTuple2<K, V>> list = new Nil<SSharpTuple2<K, V>>();
        foreach (var kv in _inner.Reverse())
        {
            list = new Cons<SSharpTuple2<K, V>>(new SSharpTuple2<K, V>(kv.Key, kv.Value), list);
        }
        return list;
    }

    // ── Display ───────────────────────────────────────────────────────────────

    public override string ToString()
    {
        var entries = _inner.Select(kv => $"{kv.Key} -> {kv.Value}");
        return $"Map({string.Join(", ", entries)})";
    }

    public override bool Equals(object? obj)
    {
        if (obj is SSharpMap<K, V> other)
        {
            return _inner.Count == other._inner.Count &&
                   _inner.All(kv => other._inner.TryGetValue(kv.Key, out var v) && Equals(v, kv.Value));
        }
        return false;
    }

    public override int GetHashCode() => _inner.GetHashCode();
}
