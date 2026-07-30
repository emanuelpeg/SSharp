# SSharp Language Syntax Reference

**SSharp** is a statically-typed, expression-based functional programming language that transpiles to executable C# on the .NET runtime. Inspired by the functional subset of Scala, it combines the expressiveness of the functional paradigm with the interoperability and performance of the .NET runtime.

---

## Table of Contents

1. [Type System](#1-type-system)
2. [Values and Bindings (`val` / `lazy val`)](#2-values-and-bindings-val--lazy-val)
3. [Functions (`def`)](#3-functions-def)
4. [Lambda Expressions](#4-lambda-expressions)
5. [Control Flow and Blocks](#5-control-flow-and-blocks)
6. [Algebraic Data Types (ADTs)](#6-algebraic-data-types-adts)
7. [Pattern Matching (`match` / `case`)](#7-pattern-matching-match--case)
8. [Lists and Collections](#8-lists-and-collections)
9. [Runtime Library (`SSharp.Runtime`)](#9-runtime-library-ssharpruntime)
10. [Comments and Imports](#10-comments-and-imports)
11. [Complete Integrated Example](#11-complete-integrated-example)
12. [CLI and REPL Tools](#12-cli-and-repl-tools)

---

## 1. Type System

SSharp has a static type system with automatic type inference.

### Primitive Types

| SSharp Type | C# Equivalent | Description | Literal Example |
|-------------|---------------|-------------|-----------------|
| `Int` | `int` | Signed 32-bit integer | `42`, `-10` |
| `Double` | `double` | 64-bit floating-point | `3.14159`, `-0.5` |
| `String` | `string` | Character string | `"Hello world"` |
| `Boolean` | `bool` | Boolean value | `true`, `false` |
| `Unit` | `SSharp.Runtime.Unit` | Type with a single value `()` (represents the absence of a returned value) | `()` |
| `Any` | `object` | Root type of the type hierarchy | Any value |

### Compound and Generic Types

- **Lists**: `List[A]`
- **Options**: `Option[A]`
- **Sets**: `Set[A]`
- **Maps**: `Map[K, V]`
- **Tuples**: `Tuple2[A, B]`
- **Functions**: `(A, B) => C` (a function that takes arguments of type `A` and `B` and returns `C`)

---

## 2. Values and Bindings (`val` / `lazy val`)

In SSharp, all bindings are immutable by default.

### Immutable Bindings (`val`)

Declared with the `val` keyword. The type may be explicit or inferred:

```scala
// With automatically inferred type
val x = 42
val message = "Hello SSharp"

// With explicit type
val pi: Double = 3.14159265359
val active: Boolean = true
```

### Lazy / Memoized Bindings (`lazy val`)

A value declared with `lazy val` is evaluated only the first time it is accessed. The result is cached (memoized) and reused on subsequent accesses:

```scala
lazy val expensiveComputation: Int = {
    println("Computing complex result...")
    10 * 20 * 30
}

println("Before access")
println(expensiveComputation) // Prints "Computing complex result..." then "6000"
println(expensiveComputation) // Prints only "6000" (cached value is reused)
```

---

## 3. Functions (`def`)

Functions are defined using the `def` keyword.

### Basic Syntax

```scala
def add(a: Int, b: Int): Int = a + b

def greet(name: String): String = "Hello, " + name
```

### Functions with Code Blocks

If the function body requires multiple statements, it is enclosed in a `{ }` block. The value of the last expression is automatically returned:

```scala
def calculateTriangleArea(base: Double, height: Double): Double = {
    val area = (base * height) / 2.0
    area
}
```

### Generic Functions

Functions can accept generic type parameters enclosed in brackets `[T]`:

```scala
def identity[T](x: T): T = x

def first[A, B](a: A, b: B): A = a
```

### Multiple Parameter Lists (Currying) and Partial Application

SSharp allows defining functions with multiple parameter lists. This enables partial application of functions:

```scala
// Curried function
def addCurried(x: Int)(y: Int): Int = x + y

// Partial application (returns a function of type Int => Int)
val addFive = addCurried(5)

val result = addFive(10) // 15
```

### By-Name Parameters (Lazy Parameters)

A parameter with the `=> T` notation is evaluated every time it is referenced inside the function body (call-by-name):

```scala
def evaluateIfTrue(condition: Boolean, expression: => Int): Int = {
    if (condition) expression else 0
}
```

### Recursion and `@tailrec` Optimization

For pure recursive functions, the `@tailrec` annotation instructs the compiler to verify that the recursive call is in tail position. The compiler will optimize it, transforming it into an imperative loop in C# that is free from stack overflow:

```scala
@tailrec
def factorialTailrec(n: Int, accumulator: Int): Int = {
    if (n <= 1) accumulator
    else factorialTailrec(n - 1, n * accumulator)
}

def factorial(n: Int): Int = factorialTailrec(n, 1)
```

---

## 4. Lambda Expressions

Anonymous functions (lambdas) are defined using the fat arrow `=>`:

```scala
// Simple lambda
val double = (x: Int) => x * 2

// Lambda with multiple parameters
val multiply = (x: Int, y: Int) => x * y

// Passing a lambda to a higher-order function
val list = List(1, 2, 3, 4)
val doubled = list.map((x: Int) => x * 2)
```

---

## 5. Control Flow and Blocks

SSharp is an expression-based language: **everything in SSharp produces a value**.

### `if` / `else` Expression

The `if` structure must always include an `else` branch and returns the value of the executed branch:

```scala
val x = 10
val sign: String = if (x >= 0) "Positive" else "Negative"

val maximum = if (a > b) a else b
```

### Block Expressions (`{ }`)

A block of code enclosed in braces returns the value of its last expression:

```scala
val result: Int = {
    val a = 10
    val b = 20
    a + b // Block return value: 30
}
```

### Operators

- **Arithmetic**: `+`, `-`, `*`, `/`, `%`
- **Relational**: `==`, `!=`, `<`, `<=`, `>`, `>=`
- **Logical**: `&&`, `||`, `!`

---

## 6. Algebraic Data Types (ADTs)

SSharp supports Algebraic Data Types via `sealed trait`, `case class`, and `case object`.

### Sealed Traits (`sealed trait`)

Defines the base interface or root algebraic type. All its subtypes must be defined in the same file:

```scala
sealed trait Shape
```

### Case Classes (`case class`)

Represent data constructors with immutable fields. They automatically generate methods for value-based comparison, structured `toString`, and destructuring in pattern matching:

```scala
case class Circle(radius: Double) extends Shape
case class Rectangle(width: Double, height: Double) extends Shape
```

### Case Objects (`case object`)

Represent singleton values or parameter-less cases (e.g., an empty case or a constant state):

```scala
case object EmptyShape extends Shape
```

### Generic ADTs

Traits and case classes can be generic:

```scala
sealed trait Option[A]
case class Some[A](value: A) extends Option[A]
case class None[A]() extends Option[A]
```

---

## 7. Pattern Matching (`match` / `case`)

Pattern matching inspects data structures and evaluates the matching branch.

### Basic Syntax

```scala
def describeShape(s: Shape): String = s match {
    case Circle(r)       => "Circle with radius " + r
    case Rectangle(w, h) => "Rectangle " + w + "x" + h
    case EmptyShape      => "Empty shape"
}
```

### Supported Pattern Types

1. **Wildcard Pattern (`_`)**: Matches any value without binding it to a name.
2. **Literal Pattern**: Matches concrete values (`42`, `"hello"`, `true`).
3. **Identifier / Variable Pattern**: Captures the value into a local variable.
4. **Constructor Pattern**: Destructures a `case class`.
5. **Infix List Pattern (`head :: tail`)**: Separates the head and tail of a list.

```scala
def process(x: Any): String = x match {
    case 0       => "It is the number zero"
    case "admin" => "It is the admin user"
    case n       => "It is some other value: " + n
    case _       => "Unknown value"
}
```

---

## 8. Lists and Collections

SSharp provides immutable and persistent data structures.

### List Construction

Lists can be constructed using the `List(...)` factory or through the infix prepend operator `::` (cons) together with `Nil`:

```scala
import "SSharp.Runtime"

// Construction using the factory
val l1 = List(1, 2, 3)

// Construction with cons (::) and Nil (right-associative)
val l2 = 1 :: 2 :: 3 :: Nil
```

### Pattern Matching on Lists

```scala
def sumElements(list: List[Int]): Int = list match {
    case Nil        => 0
    case head::tail => head + sumElements(tail)
}
```

---

## 9. Runtime Library (`SSharp.Runtime`)

To use the SSharp standard runtime, import the module:

```scala
import "SSharp.Runtime"
```

### Built-in I/O Functions

- `print(x: Any): Unit`: Prints to the console without a newline.
- `println(x: Any): Unit`: Prints to the console appending a newline.
- `readLine(): String`: Reads a line of text entered by the user from the console.

### Higher-Order Methods on Collections

SSharp's immutable lists and collections include standard functional methods:

```scala
val numbers = List(1, 2, 3, 4, 5)

// map: Transforms each element
val doubles = numbers.map((x: Int) => x * 2)

// filter: Filters elements according to a predicate
val evens = numbers.filter((x: Int) => x % 2 == 0)

// foldLeft: Reduces the list by accumulating a value from the left
val total = numbers.foldLeft(0, (acc: Int, x: Int) => acc + x)
```

---

## 10. Comments and Imports

### Comments

```scala
// This is a single-line comment

/*
   This is a
   multi-line comment
*/
```

### Imports

To import .NET / SSharp namespaces or modules:

```scala
import "SSharp.Runtime"
```

---

## 11. Complete Integrated Example

```scala
import "SSharp.Runtime"

// Data hierarchy definition (ADT)
sealed trait Tree[A]
case class Leaf[A](value: A) extends Tree[A]
case class Node[A](left: Tree[A], right: Tree[A]) extends Tree[A]

// Recursive function with pattern matching
def countLeaves[A](tree: Tree[A]): Int = tree match {
    case Leaf(_)     => 1
    case Node(l, r)  => countLeaves(l) + countLeaves(r)
}

// Curried function
def multiplyBy(factor: Int)(number: Int): Int = factor * number

def main(): Unit = {
    val myTree = Node(Leaf(10), Node(Leaf(20), Leaf(30)))
    val totalLeaves = countLeaves(myTree)

    val timesTen = multiplyBy(10)
    val result = timesTen(5)

    println("Total leaves in tree: " + totalLeaves)
    println("10 * 5 = " + result)
}
```

---

## 12. CLI and REPL Tools

### Using the CLI Compiler (`SSharp.CLI`)

```sh
# Transpile a .ss file to C#
dotnet run --project SSharp.CLI -- program.ss

# Transpile and compile to a runnable .NET .dll
dotnet run --project SSharp.CLI -- program.ss -c

# Transpile, compile, and run immediately
dotnet run --project SSharp.CLI -- program.ss -r
```

### Using the Interactive REPL

To start the interactive console:

```sh
dotnet run --project SSharp.CLI -- repl
```

**Example REPL session:**

```text
ssharp> 2 + 2
res0 = 4 : Int

ssharp> val name = "SSharp"
name = "SSharp" : String

ssharp> List(1, 2, 3)
res1 = List(1, 2, 3) : List[Int]
```

Useful commands inside the REPL:
- `:help` / `:h` — Show the help menu.
- `:quit` / `:q` — Exit the interactive session.
- `:reset` — Reset the accumulated session context.
- `:ctx` — Show the source code accumulated in the current context.
