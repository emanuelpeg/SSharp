using System;
using System.Collections.Generic;
using System.Linq;
using SSharp.Compiler;
using Xunit;

namespace SSharp.Tests;

public class TranspilerTests
{
    private string Transpile(string sourceCode)
    {
        var lexer = new Lexer(sourceCode);
        var tokens = lexer.ScanTokens();
        
        var lexerErrors = tokens.Where(t => t.Type == TokenType.Error).ToList();
        if (lexerErrors.Any())
        {
            throw new Exception("Lexer errors:\n" + string.Join("\n", lexerErrors.Select(e => e.Value)));
        }

        var parser = new Parser(tokens);
        var ast = parser.ParseProgram();
        if (parser.Errors.Any())
        {
            throw new Exception("Parser errors:\n" + string.Join("\n", parser.Errors));
        }

        var typeChecker = new TypeChecker();
        typeChecker.Check(ast);
        if (typeChecker.Errors.Any())
        {
            throw new Exception("Type Checker errors:\n" + string.Join("\n", typeChecker.Errors));
        }

        var generator = new CodeGenerator(typeChecker.ResolvedTypes);
        return generator.Generate(ast).Replace("\r\n", "\n").Trim();
    }

    [Fact]
    public void TestBasicArithmeticAndVal()
    {
        string source = "val x: Int = 10 + 20 * 30;";
        string output = Transpile(source);
        
        Assert.Contains("public static readonly int x = (10 + (20 * 30));", output);
    }

    [Fact]
    public void TestIfExpression()
    {
        string source = "val result: Int = if (true) 1 else 2;";
        string output = Transpile(source);
        
        Assert.Contains("public static readonly int result = (true ? 1 : 2);", output);
    }

    [Fact]
    public void TestBlockExpression()
    {
        string source = "val result: Int = { val y = 5; y + 10 };";
        string output = Transpile(source);
        
        // Block expressions translate to Immediately Invoked Function Expressions (IIFE)
        Assert.Contains("new System.Func<int>(() =>", output);
        Assert.Contains("var y = 5;", output);
        Assert.Contains("return (y + 10);", output);
        Assert.Contains("})()", output);
    }

    [Fact]
    public void TestFunctionDeclaration()
    {
        string source = "def add(a: Int, b: Int): Int = a + b;";
        string output = Transpile(source);

        Assert.Contains("public static int add(int a, int b) => (a + b);", output);
    }

    [Fact]
    public void TestFunctionWithBlockBody()
    {
        string source = "def compute(a: Int): Int = { val x = a * 2; x + 5 };";
        string output = Transpile(source);

        // A block-bodied function has standard braces, not an IIFE wrapper at the top level
        Assert.Contains("public static int compute(int a)", output);
        Assert.Contains("var x = (a * 2);", output);
        Assert.Contains("return (x + 5);", output);
        Assert.DoesNotContain("new System.Func<int>(() =>", output);
    }

    [Fact]
    public void TestAlgebraicDataTypes()
    {
        string source = @"
            sealed trait Shape;
            case class Circle(radius: Double) extends Shape;
            case class Rectangle(width: Double, height: Double) extends Shape;
            case object EmptyShape extends Shape;
        ";
        string output = Transpile(source);

        Assert.Contains("public abstract record Shape;", output);
        Assert.Contains("public record Circle(double radius) : Shape;", output);
        Assert.Contains("public record Rectangle(double width, double height) : Shape;", output);
        Assert.Contains("public record EmptyShape : Shape", output);
        Assert.Contains("public static EmptyShape Instance { get; } = new EmptyShape();", output);

        // Check factory methods inside Program class
        Assert.Contains("public static Circle Circle(double radius) => new Circle(radius);", output);
        Assert.Contains("public static Rectangle Rectangle(double width, double height) => new Rectangle(width, height);", output);
    }

    [Fact]
    public void TestLambdaExpression()
    {
        string source = "val f = (x: Int) => x + 1;";
        string output = Transpile(source);

        Assert.Contains("public static readonly System.Func<int, int> f = new System.Func<int, int>((x) => (x + 1));", output);
    }

    [Fact]
    public void TestPatternMatchingOnADTs()
    {
        string source = @"
            sealed trait Shape;
            case class Circle(radius: Double) extends Shape;
            case class Rectangle(width: Double, height: Double) extends Shape;

            def area(s: Shape): Double = s match {
                case Circle(r) => 3.14 * r * r
                case Rectangle(w, h) => w * h
            };
        ";
        string output = Transpile(source);

        Assert.Contains("public static double area(Shape s) => (s) switch", output);
        Assert.Contains("Circle(var r) => ((3.14d * r) * r),", output);
        Assert.Contains("Rectangle(var w, var h) => (w * h),", output);
        Assert.Contains("_ => throw new System.InvalidOperationException(\"Pattern match failed\")", output);
    }

    [Fact]
    public void TestPatternMatchingOnRuntimeList()
    {
        // Nil and Cons are mapped to SSharp.Runtime classes
        string source = @"
            import ""SSharp.Runtime"";
            
            def length[A](list: List[A]): Int = list match {
                case Nil => 0
                case Cons(head, tail) => 1 + length(tail)
            };
        ";
        string output = Transpile(source);

        Assert.Contains("public static int length<A>(SSharp.Runtime.SSharpList<A> list) => (list) switch", output);
        Assert.Contains("SSharp.Runtime.Nil<A> => 0,", output);
        Assert.Contains("SSharp.Runtime.Cons<A>(var head, var tail) => (1 + length(tail)),", output);
        Assert.Contains("_ => throw new System.InvalidOperationException(\"Pattern match failed\")", output);
    }

    [Fact]
    public void TestInfixListPatternAndListFactory()
    {
        string source = @"
            import ""SSharp.Runtime"";
            
            def len[T](l: List[T]): Int = l match {
                case Nil => 0
                case head::tail => 1 + len(tail)
            };

            def main(): Unit = {
                val myList = List(1, 2, 3);
                print(len(myList));
            };
        ";
        string output = Transpile(source);

        Assert.Contains("public static int len<T>(SSharp.Runtime.SSharpList<T> l) => (l) switch", output);
        Assert.Contains("SSharp.Runtime.Nil<T> => 0,", output);
        Assert.Contains("SSharp.Runtime.Cons<T>(var head, var tail) => (1 + len(tail)),", output);
        Assert.Contains("var myList = List(1, 2, 3);", output);
    }

    [Fact]
    public void TestTailrecOptimization()
    {
        string source = @"
            import ""SSharp.Runtime"";

            @tailrec
            def sumList(l: List[Int], acc: Int): Int = l match {
                case Nil => acc
                case head::tail => sumList(tail, acc + head)
            };
        ";
        string output = Transpile(source);

        Assert.Contains("while (true)", output);
        Assert.Contains("SSharp.Runtime.SSharpList<int> _tailrec_temp_l_0 = tail;", output);
        Assert.Contains("int _tailrec_temp_acc_1 = (acc + head);", output);
        Assert.Contains("l = _tailrec_temp_l_0;", output);
        Assert.Contains("acc = _tailrec_temp_acc_1;", output);
        Assert.Contains("continue;", output);
    }

    [Fact]
    public void TestTailrecValidationErrors()
    {
        // 1. Not recursive
        string source1 = @"
            @tailrec
            def add(a: Int, b: Int): Int = a + b;
        ";
        var ex1 = Assert.Throws<Exception>(() => Transpile(source1));
        Assert.Contains("it contains no recursive calls", ex1.Message);

        // 2. Recursive but not in tail position
        string source2 = @"
            @tailrec
            def factorial(n: Int): Int =
                if (n <= 1) 1 else n * factorial(n - 1);
        ";
        var ex2 = Assert.Throws<Exception>(() => Transpile(source2));
        Assert.Contains("Recursive call to 'factorial' is not in tail position", ex2.Message);
    }

    [Fact]
    public void TestLazyParameters()
    {
        string source = @"
            def and(a: Boolean, b: => Boolean): Boolean =
                if (a) b else false;

            def andThin(a: Boolean, b: -> Boolean): Boolean =
                if (a) b else false;

            def main(): Unit = {
                val res = and(true, true);
                val res2 = andThin(false, true);
            };
        ";
        string output = Transpile(source);

        Assert.Contains("public static bool and(bool a, System.Func<bool> b) => (a ? b() : false);", output);
        Assert.Contains("public static bool andThin(bool a, System.Func<bool> b) => (a ? b() : false);", output);
        Assert.Contains("and(true, new System.Func<bool>(() => true))", output);
        Assert.Contains("andThin(false, new System.Func<bool>(() => true))", output);
    }

    [Fact]
    public void TestLazyValModifier()
    {
        string source = @"
            lazy val x: Int = 10 + 20;

            def getLazy(): Int = {
                lazy val y = x * 2;
                y + 5
            };
        ";
        string output = Transpile(source);

        Assert.Contains("private static readonly System.Lazy<int> _lazy_field_x = new System.Lazy<int>(() => (10 + 20));", output);
        Assert.Contains("public static int x => _lazy_field_x.Value;", output);

        Assert.Contains("var y = new System.Lazy<int>(() => (x * 2));", output);
        Assert.Contains("return (y.Value + 5);", output);
    }
}
