using System;
using System.Collections.Generic;
using SSharp.Runtime;
using static SSharp.Runtime.Predef;

namespace SSharp.Generated;

public static class Program
{
    public static string evaluar(int n) => (n) switch
{
    var x when ((x < 0)) => "Negativo",
    0 => "Cero",
    var x when (((x % 2) == 0)) => "Positivo par",
    _ => "Positivo impar",
};
    public static string describeFirst(SSharp.Runtime.SSharpList<int> l) => (l) switch
{
    SSharp.Runtime.Cons<int>(var h, var t) and var original => ((("first=" + h) + " size=+") + h),
    SSharp.Runtime.Nil<int> => "empty",
    _ => throw new System.InvalidOperationException("Pattern match failed")
};
    public static SSharp.Runtime.Unit main()
    {
        println(evaluar((-5)));
        println(evaluar(0));
        println(evaluar(4));
        println(evaluar(7));
        return println(describeFirst(List(1, 2, 3)));
    }
    public static void Main(string[] args)
    {
        main();
    }
}
