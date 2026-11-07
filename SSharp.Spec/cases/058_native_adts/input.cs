using System;
using System.Collections.Generic;
using SSharp.Runtime;
using static SSharp.Runtime.Predef;

namespace SSharp.Generated;

public interface Shape {}

public record Circle(double radius) : Shape;
public record Rectangle(double width, double height) : Shape;
public record Dot : Shape
{
    private Dot() {}
    public static Dot Instance { get; } = new Dot();
}

public static class Program
{
    public static Circle Circle(double radius) => new Circle(radius);
    public static double radius(Shape obj) => obj is Circle c ? c.radius : throw new System.InvalidOperationException("Field 'radius' does not exist on " + obj.GetType().Name);
    public static Rectangle Rectangle(double width, double height) => new Rectangle(width, height);
    public static double width(Shape obj) => obj is Rectangle c ? c.width : throw new System.InvalidOperationException("Field 'width' does not exist on " + obj.GetType().Name);
    public static double height(Shape obj) => obj is Rectangle c ? c.height : throw new System.InvalidOperationException("Field 'height' does not exist on " + obj.GetType().Name);

    public static double area(Shape s) => (s) switch
{
    Circle(var r) => ((3.14d * r) * r),
    Rectangle(var w, var h) => (w * h),
    Dot => 0d,
    _ => throw new System.InvalidOperationException("Pattern match failed")
};
    public static SSharp.Runtime.Unit main()
    {
        var c = Circle(10d);
        var r = Rectangle(4d, 5d);
        var d = Dot.Instance;
        println(area(c));
        println(area(r));
        println(area(d));
        println(radius(c));
        println(width(r));
        return println(height(r));
    }
    public static void Main(string[] args)
    {
        main();
    }
}
