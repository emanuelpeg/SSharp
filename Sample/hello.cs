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
    public static SSharp.Runtime.Unit main()
    {
        var c = Circle(5d);
        var r = Rectangle(4d, 6d);
        println(("Circle area: " + area(c)));
        println(("Rectangle area: " + area(r)));
        return println(("5! = " + factorial(5)));
    }
    public static void Main(string[] args)
    {
        main();
    }
}
