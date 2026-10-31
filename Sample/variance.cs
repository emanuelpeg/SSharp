using System;
using System.Collections.Generic;
using SSharp.Runtime;
using static SSharp.Runtime.Predef;

namespace SSharp.Generated;

public interface Animal {}

public record Cat(string name) : Animal;

public record Dog(string name) : Animal;

public interface Container<A> {}

public record Box<A>(A value) : Container<A>;

public interface Shape {}

public record Circle(double radius) : Shape;

public record Rectangle(double width, double height) : Shape;

public static class Program
{
    public static Cat Cat(string name) => new Cat(name);
    public static Dog Dog(string name) => new Dog(name);
    public static Box<A> Box<A>(A value) => new Box<A>(value);
    public static Circle Circle(double radius) => new Circle(radius);
    public static Rectangle Rectangle(double width, double height) => new Rectangle(width, height);

    public static string animalSound(Animal a) => (a) switch
{
    Cat(var n) => (n + " dice: Miau"),
    Dog(var n) => (n + " dice: Guau"),
    _ => throw new System.InvalidOperationException("Pattern match failed")
};
    public static A unbox<A>(Container<A> b) => (b) switch
{
    Box<A>(var v) => v,
    _ => throw new System.InvalidOperationException("Pattern match failed")
};
    public static double area(Shape s) => (s) switch
{
    Circle(var r) => ((3.14159265d * r) * r),
    Rectangle(var w, var h) => (w * h),
    _ => throw new System.InvalidOperationException("Pattern match failed")
};
    public static SSharp.Runtime.Unit main()
    {
        var michi = Cat("Michi");
        var firulais = Dog("Firulais");
        println(animalSound(michi));
        println(animalSound(firulais));
        var intBox = Box(42);
        var strBox = Box("SSharp Funcional");
        println(("Box int: " + unbox(intBox)));
        println(("Box str: " + unbox(strBox)));
        var c = Circle(5d);
        var r = Rectangle(4d, 6d);
        println(("Área círculo: " + area(c)));
        return println(("Área rectángulo: " + area(r)));
    }
    public static void Main(string[] args)
    {
        main();
    }
}
