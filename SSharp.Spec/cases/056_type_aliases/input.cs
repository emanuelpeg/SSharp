using System;
using System.Collections.Generic;
using SSharp.Runtime;
using static SSharp.Runtime.Predef;

namespace SSharp.Generated;

public static class Program
{
    public static bool isEven(int x) => ((x % 2) == 0);
    public static int @double(int x) => (x * 2);
    public static string greet(string name) => ("Hello, " + name);
    public static bool applyPred(System.Func<int, bool> p, int x) => p(x);
    public static int applyTransform(System.Func<int, int> f, int x) => f(x);
    public static SSharp.Runtime.Unit main()
    {
        var evens = filter(isEven, List(1, 2, 3, 4, 5, 6));
        println(evens);
        var doubled = map(@double, List(1, 2, 3));
        println(doubled);
        var msg = greet("World");
        println(msg);
        var label = "origin";
        println(label);
        var r1 = applyPred(isEven, 4);
        var r2 = applyTransform(@double, 7);
        println(r1);
        return println(r2);
    }
    public static void Main(string[] args)
    {
        main();
    }
}
