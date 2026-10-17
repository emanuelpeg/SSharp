using System;
using System.Collections.Generic;
using SSharp.Runtime;
using static SSharp.Runtime.Predef;

namespace SSharp.Generated;

public static class Program
{
    public static SSharp.Runtime.Unit main()
    {
        var pares = flatMap((x) => map((y) => (x + y), filter((y) => ((x + y) > 15), List(10, 20))), List(1, 2, 3));
        println(("pares: " + pares));
        var combinaciones = flatMap((n) => map((s) => new SSharp.Runtime.SSharpTuple2<int, string>(n, s), List("a", "b")), filter((n) => ((n % 2) != 0), List(1, 2, 3)));
        println(("combinaciones: " + combinaciones));
        var optA = Some<int>(10);
        var optB = Some<int>(20);
        var sumaOpt = flatMap((a) => map((b) => (a + b), optB), optA);
        return println(("sumaOpt: " + sumaOpt));
    }
    public static void Main(string[] args)
    {
        main();
    }
}
