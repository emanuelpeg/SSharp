using System;

namespace SSharp.Runtime;

public abstract record SSharpEither<L, R>
{
    public abstract bool IsLeft { get; }
    public abstract bool IsRight { get; }

    public SSharpEither<L, B> Map<B>(Func<R, B> f) =>
        this is Right<L, R> r ? new Right<L, B>(f(r.Value)) : new Left<L, B>(((Left<L, R>)this).Value);

    public SSharpEither<A, R> MapLeft<A>(Func<L, A> f) =>
        this is Left<L, R> l ? new Left<A, R>(f(l.Value)) : new Right<A, R>(((Right<L, R>)this).Value);

    public SSharpEither<L, B> FlatMap<B>(Func<R, SSharpEither<L, B>> f) =>
        this is Right<L, R> r ? f(r.Value) : new Left<L, B>(((Left<L, R>)this).Value);

    public R GetOrElse(Func<R> @default) =>
        this is Right<L, R> r ? r.Value : @default();

    public T Fold<T>(Func<L, T> fa, Func<R, T> fb) =>
        this is Left<L, R> l ? fa(l.Value) : fb(((Right<L, R>)this).Value);

    public SSharpOption<R> ToOption() =>
        this is Right<L, R> r ? new Some<R>(r.Value) : new None<R>();
}

public record Left<L, R>(L Value) : SSharpEither<L, R>
{
    public override bool IsLeft => true;
    public override bool IsRight => false;
    public override string ToString() => $"Left({Value})";
}

public record Right<L, R>(R Value) : SSharpEither<L, R>
{
    public override bool IsLeft => false;
    public override bool IsRight => true;
    public override string ToString() => $"Right({Value})";
}
