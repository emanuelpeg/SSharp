using System;

namespace SSharp.Runtime;

public abstract record SSharpOption<T>
{
    public abstract bool IsDefined { get; }
    public abstract T Get { get; }
    public T GetOrElse(Func<T> @default) => IsDefined ? Get : @default();

    public SSharpOption<U> Map<U>(Func<T, U> f)
    {
        if (this is Some<T> some)
        {
            return new Some<U>(f(some.Value));
        }
        return new None<U>();
    }

    public SSharpOption<T> Filter(Func<T, bool> p)
    {
        if (this is Some<T> some && p(some.Value))
        {
            return this;
        }
        return new None<T>();
    }
}

public record None<T> : SSharpOption<T>
{
    public override bool IsDefined => false;
    public override T Get => throw new InvalidOperationException("None.Get");
    public override string ToString() => "None";
}

public record Some<T>(T Value) : SSharpOption<T>
{
    public override bool IsDefined => true;
    public override T Get => Value;
    public override string ToString() => $"Some({Value})";
}
