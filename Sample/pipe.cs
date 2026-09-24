using System;
using System.Collections.Generic;
using SSharp.Runtime;
using static SSharp.Runtime.Predef;

namespace SSharp.Generated;

public static class Program
{
    public static bool isEven(int n) => ((n % 2) == 0);
    public static int doubleIt(int n) => (n * 2);
    public static int add(int a, int b) => (a + b);
    public static SSharp.Runtime.Unit main()
    {
        var xs = List(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        var resultado = take(5, map(doubleIt, filter(isEven, xs)));
        println(("xs: " + xs));
        println(("resultado: " + resultado));
        var total = sum(map(doubleIt, filter(isEven, xs)));
        println(("total suma: " + total));
        var calc = add(10, doubleIt(5));
        return println(("5 |> doubleIt |> add(10) = " + calc));
    }
    public static void Main(string[] args)
    {
        main();
    }
}
