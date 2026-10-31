using System;
using System.Collections.Generic;
using SSharp.Runtime;
using static SSharp.Runtime.Predef;

namespace SSharp.Generated;

public interface Shape {}

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
    public static int len<T>(SSharp.Runtime.SSharpList<T> l, int @default)
    {
        while (true)
        {
            switch (l)
            {
                case SSharp.Runtime.Nil<T>:
                    {
                        return @default;
                    }
                case SSharp.Runtime.Cons<T>(var head, var tail):
                    {
                        SSharp.Runtime.SSharpList<T> _tailrec_temp_l_0 = tail;
                        int _tailrec_temp_default_1 = (@default + 1);
                        l = _tailrec_temp_l_0;
                        @default = _tailrec_temp_default_1;
                        continue;
                    }
                default:
                    throw new System.InvalidOperationException("Pattern match failed");
            }
        }
    }
    public static int lenDef<T>(SSharp.Runtime.SSharpList<T> l) => len(l, 0);
    public static int sum(int x, int y) => (x + y);
    public static int evaluateIf(bool condition, System.Func<int> expression) => (condition ? expression() : 0);
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
        println(("head(l) = " + head(l)));
        println(("tail(l) = " + tail(l)));
        println(("add 2 to 3 = " + add2(3)));
        var resultAdd2 = ((add2(10) == 12) ? "Yes" : "No");
        println(("add 2 to 10 = " + resultAdd2));
        println("Testing pure lazy parameters (call-by-name):");
        var zero = 0;
        var lazyZero = evaluateIf(false, new System.Func<int>(() => (100 / zero)));
        println(("lazyZero (safe without evaluating): " + lazyZero));
        var lazyVal = evaluateIf(true, new System.Func<int>(() => (10 * 5)));
        return println(("lazyVal (evaluated when true): " + lazyVal));
    }
    public static void Main(string[] args)
    {
        main();
    }
}
