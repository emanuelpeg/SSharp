using System;
using System.Collections.Generic;
using SSharp.Runtime;
using static SSharp.Runtime.Predef;

namespace SSharp.Generated;

public abstract record Shape;

public record Circle(double radius) : Shape;

public record Rectangle(double width, double height) : Shape;

public static class Program
{
    public static Circle Circle(double radius) => new Circle(radius);
    public static Rectangle Rectangle(double width, double height) => new Rectangle(width, height);

    public static double area(Shape s) => (s) switch
{
    Circle(var r) => ((3.14159d * r) * r),
    Rectangle(var w, var h) => (w * h),
    _ => throw new System.InvalidOperationException("Pattern match failed")
};
    public static int factorial(int n) => ((n <= 1) ? 1 : (n * factorial((n - 1))));
    public static int len<T>(SSharp.Runtime.SSharpList<T> l, int @default) => (l) switch
{
    SSharp.Runtime.Nil<T> => @default,
    SSharp.Runtime.Cons<T>(var head, var tail) => (1 + len(tail, @default)),
    _ => throw new System.InvalidOperationException("Pattern match failed")
};
    public static int lenDef<T>(SSharp.Runtime.SSharpList<T> l) => len(l, 0);
    public static int sum(int x, int y) => (x + y);
    public static SSharp.Runtime.Unit main()
    {
        var c = Circle(5d);
        var r = Rectangle(4d, 6d);
        var l = List(1, 2, 3);
        var add2 = new System.Func<int, int>((_p0) => sum(2, _p0));
        println(("Circle area: " + area(c)));
        println(("Rectangle area: " + area(r)));
        println(("5! = " + factorial(5)));
        println(("length of list [1, 2, 3] = " + len(l, 0)));
        println(("length of list [1, 2, 3] = " + lenDef(l)));
        return println(("add 2 to 3= " + add2(3)));
    }
    public static void Main(string[] args)
    {
        main();
    }
}
