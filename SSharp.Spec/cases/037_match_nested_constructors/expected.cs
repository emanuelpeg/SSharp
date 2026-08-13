using System;

public interface Expr {}
public record Num(int n) : Expr;
public record Add(Expr e1, Expr e2) : Expr;

public static class Program
{
    public static int eval(Expr e) => e switch {
        Num(var n) => n,
        Add(var e1, var e2) => eval(e1) + eval(e2),
        _ => throw new InvalidOperationException()
    };

    public static void Main()
    {
        Expr expr = new Add(new Num(10), new Add(new Num(20), new Num(30)));
        Console.WriteLine(eval(expr));
    }
}
