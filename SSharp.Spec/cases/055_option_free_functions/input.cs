using System;
using System.Collections.Generic;
using SSharp.Runtime;
using static SSharp.Runtime.Predef;

namespace SSharp.Generated;

public static class Program
{
    public static SSharp.Runtime.Unit main()
    {
        var s = Some<int>(42);
        var n = new SSharp.Runtime.None<int>();
        println(isSome(s));
        println(isNone(s));
        println(isDefined(s));
        println(isEmpty(s));
        println(isSome(n));
        println(isNone(n));
        println(isDefined(n));
        println(isEmpty(n));
        println(getOrElse(0, s));
        println(getOrElse(0, n));
        var mappedSome = map(new System.Func<int, int>((x) => (x * 2)), s);
        var mappedNone = map(new System.Func<int, int>((x) => (x * 2)), n);
        println(mappedSome);
        println(mappedNone);
        var filteredKeep = filter(new System.Func<int, bool>((x) => (x > 40)), s);
        var filteredDrop = filter(new System.Func<int, bool>((x) => (x > 50)), s);
        println(filteredKeep);
        println(filteredDrop);
        var nestedSome = Some<SSharp.Runtime.SSharpOption<int>>(Some<int>(99));
        var nestedNone = Some<SSharp.Runtime.SSharpOption<object>>(new SSharp.Runtime.None<object>());
        println(flatten(nestedSome));
        return println(flatten(nestedNone));
    }
    public static void Main(string[] args)
    {
        main();
    }
}
