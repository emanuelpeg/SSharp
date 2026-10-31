using System;
using System.Collections.Generic;
using SSharp.Runtime;
using static SSharp.Runtime.Predef;

namespace SSharp.Generated;

public static class Program
{
    public static SSharp.Runtime.Unit main()
    {
        var xs = List(1, 2, 3, 4, 5);
        var h = head(xs);
        var t = tail(xs);
        println(h);
        println(t);
        var l = last(xs);
        var i = init(xs);
        println(l);
        println(i);
        var empty = tail(List(0));
        var hSome = headOption(xs);
        var hNone = headOption(empty);
        println(hSome);
        println(hNone);
        var tSome = tailOption(xs);
        var tNone = tailOption(empty);
        println(tSome);
        println(tNone);
        var u = uncons(xs);
        var uNone = uncons(empty);
        println(u);
        return println(uNone);
    }
    public static void Main(string[] args)
    {
        main();
    }
}
