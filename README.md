# SSharp

**SSharp** — Functional Programming for the CLR.

SSharp is a statically-typed, expression-based functional language that transpiles to C# and runs on the .NET runtime. Inspired by the functional subset of Scala, SSharp features algebraic data types, pattern matching, immutable bindings, and higher-order functions — all compiled down to idiomatic C# records and switch expressions.

---

## Features

- **Immutable bindings** with `val`
- **First-class functions** and lambdas
- **Algebraic Data Types** via `sealed trait`, `case class`, and `case object`
- **Pattern matching** with `match`/`case` (including constructor, literal, identifier, and wildcard patterns)
- **Expression-based** syntax — everything is an expression, including `if` and blocks `{ }`
- **Generic functions** and data types
- **Recursive functions**
- **Built-in types**: `Int`, `Double`, `String`, `Boolean`, `Unit`
- **Runtime library**: `List[A]` (singly-linked), `Option[A]`, and standard `print`/`println`/`readLine`
- **Transpiles to C#** — output is clean, human-readable C# source code

---

## Project Structure

```
SSharp/
├── SSharp.Compiler/     # Lexer, Parser, TypeChecker, CodeGenerator
├── SSharp.Runtime/      # Runtime library (List, Option, Unit, Predef)
├── SSharp.Backend/      # Roslyn-based C# → .NET assembly compiler
├── SSharp.CLI/          # Command-line compiler driver
└── SSharp.Tests/        # Unit tests for the compiler and transpiler
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Building

```sh
dotnet build
```

### Running Tests

```sh
dotnet test
```

---

## Compiling SSharp Programs

Use `SSharp.CLI` to compile a `.ss` source file:

```sh
dotnet run --project SSharp.CLI -- <input.ss> [options]
```

| Option | Description |
|--------|-------------|
| `-o <file.cs>` | Output path for the generated C# file (default: `<input>.cs`) |
| `-c`, `--compile` | Also compile the C# to a runnable .NET assembly (`.dll`) |
| `-r`, `--run` | Transpile, compile to a runnable assembly, and execute it immediately |
| `--out-dll <file.dll>` | Output path for the compiled assembly (default: `<input>.dll`) |
| `--runtime-dll <path>` | Explicit path to `SSharp.Runtime.dll` (auto-discovered by default) |

**Step 1 — Transpile to C# only:**
```sh
dotnet run --project SSharp.CLI -- hello.ss
# → hello.cs
```

**Step 2 — Transpile and compile to a .NET executable:**
```sh
dotnet run --project SSharp.CLI -- hello.ss -c
# → hello.cs  (C# source)
# → hello.dll (runnable assembly)

dotnet hello.dll
```

**Step 3 — Transpile, compile, and run immediately:**
```sh
dotnet run --project SSharp.CLI -- hello.ss -r
# → hello.cs  (C# source)
# → hello.dll (runnable assembly)
# → Runs the compiled program directly
```

---

## Language Guide

### Values

```scala
val x: Int = 42
val message = "Hello, World!"
```

### Functions

```scala
def add(a: Int, b: Int): Int = a + b

def greet(name: String): String = "Hello, " + name
```

### Lambdas

```scala
val double = (x: Int) => x * 2

val apply = (f: Int => Int, x: Int) => f(x)
```

### If Expressions

```scala
val abs: Int = if (x < 0) -x else x
```

### Block Expressions

```scala
val result: Int = {
    val a = 10
    val b = 20
    a + b
}
```

### Algebraic Data Types

```scala
sealed trait Shape
case class Circle(radius: Double) extends Shape
case class Rectangle(width: Double, height: Double) extends Shape
case object EmptyShape extends Shape
```

### Pattern Matching

```scala
def area(s: Shape): Double = s match {
    case Circle(r)          => 3.14159 * r * r
    case Rectangle(w, h)    => w * h
    case EmptyShape         => 0.0
}
```

### Recursive Functions

```scala
def factorial(n: Int): Int =
    if (n <= 1) 1 else n * factorial(n - 1)
```

### Working with Lists

```scala
import "SSharp.Runtime"

def length[A](list: List[A]): Int = list match {
    case Nil         => 0
    case Cons(_, t)  => 1 + length(t)
}
```

---

## Example: Full Program

```scala
import "SSharp.Runtime"

sealed trait Shape
case class Circle(radius: Double) extends Shape
case class Rectangle(width: Double, height: Double) extends Shape

def area(s: Shape): Double = s match {
    case Circle(r)       => 3.14159 * r * r
    case Rectangle(w, h) => w * h
}

def factorial(n: Int): Int =
    if (n <= 1) 1 else n * factorial(n - 1)

def len[T](l: List[T])(default : Int): Int = l match {
    case Nil => default
    case head::tail => 1 + len(tail, default)
}

def lenDef[T](l: List[T]): Int = len(l, 0)

def sum(x: Int)(y: Int): Int = x + y

def main(): Unit = {
    val c = Circle(5.0)
    val r = Rectangle(4.0, 6.0)
    val l = List(1, 2, 3)
    val add2 = sum(2)

    println("Circle area: " + area(c))
    println("Rectangle area: " + area(r))
    println("5! = " + factorial(5))
    println("length of list [1, 2, 3] = " + len(l, 0))
    println("length of list [1, 2, 3] = " + lenDef(l))
    println("add 2 to 3= " + add2(3))
}
```

**Generated C# output:**

```csharp
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
        _ => throw new InvalidOperationException("Pattern match failed")
    };

    public static int factorial(int n) => ((n <= 1) ? 1 : (n * factorial((n - 1))));

    public static Unit main() { ... }

    public static void Main(string[] args) => main();
}
```

---

## Type System

| SSharp Type   | C# Type                          |
|---------------|----------------------------------|
| `Int`         | `int`                            |
| `Double`      | `double`                         |
| `String`      | `string`                         |
| `Boolean`     | `bool`                           |
| `Unit`        | `SSharp.Runtime.Unit`            |
| `Any`         | `object`                         |
| `List[A]`     | `SSharp.Runtime.SSharpList<A>`   |
| `Option[A]`   | `SSharp.Runtime.SSharpOption<A>` |
| `(A) => B`    | `System.Func<A, B>`              |

---

## License

[MIT](LICENSE)
