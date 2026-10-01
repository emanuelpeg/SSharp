using System;
using System.Collections.Generic;
using SSharp.Runtime;
using static SSharp.Runtime.Predef;

namespace SSharp.Generated;

public static class Program
{
    public static readonly SSharp.Runtime.SSharpTuple3<string, int, bool> persona = new SSharp.Runtime.SSharpTuple3<string, int, bool>("Alice", 25, true);
    public static string format(SSharp.Runtime.SSharpTuple2<string, int> t) => (t) switch
{
    SSharp.Runtime.SSharpTuple2<string, int>(var nombre, var edad) => (((nombre + " tiene ") + edad) + " años"),
    _ => throw new System.InvalidOperationException("Pattern match failed")
};
    public static int sumTuple4(SSharp.Runtime.SSharpTuple4<int, int, int, int> t) => (t) switch
{
    SSharp.Runtime.SSharpTuple4<int, int, int, int>(var a, var b, var c, var d) => (((a + b) + c) + d),
    _ => throw new System.InvalidOperationException("Pattern match failed")
};
    public static SSharp.Runtime.Unit main()
    {
        println(("persona: " + persona));
        println(("Nombre: " + persona._1));
        println(("Edad: " + persona._2));
        println(("Activo: " + persona._3));
        var info = new SSharp.Runtime.SSharpTuple2<string, int>("Bob", 30);
        println(format(info));
        println(format(new SSharp.Runtime.SSharpTuple2<string, int>("Charlie", 40)));
        var t4 = new SSharp.Runtime.SSharpTuple4<int, int, int, int>(10, 20, 30, 40);
        return println(("Suma t4 (10, 20, 30, 40): " + sumTuple4(t4)));
    }
    public static void Main(string[] args)
    {
        main();
    }
}
