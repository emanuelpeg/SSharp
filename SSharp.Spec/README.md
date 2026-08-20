# SSharp.Spec — Language Specification and Test Suite

> *Looking for Spanish documentation? See [README_es.md](README_es.md).*

The **SSharp.Spec** suite defines the formal, executable specification for the **SSharp** programming language. Each test case validates the end-to-end compiler pipeline (`Lexer` → `Parser` → `TypeChecker` → `CodeGenerator` → `EvalBackend`) by evaluating SSharp source files (`.ss`) against equivalent C# reference programs (`.cs`) in isolated in-memory runtimes and verifying output parity.

---

## 🚀 Running the Specification Suite

To run all specification test cases:

```powershell
dotnet test SSharp.Spec\SSharp.Spec.csproj
```

To run with detailed test-by-test progress and outputs:

```powershell
dotnet test SSharp.Spec\SSharp.Spec.csproj --logger "console;verbosity=normal"
```

---

## 🏗️ Test Case Anatomy

Every subfolder inside [`cases/`](cases/) represents a self-contained language specification test:
- `input.ss`: The SSharp source code being verified.
- `expected.cs`: The reference C# implementation demonstrating the expected semantics and output.

The test runner [`SpecTests.cs`](SpecTests.cs) compiles and evaluates both sources using in-memory Roslyn compilation with isolated `AssemblyLoadContext` sandboxes and asserts:
```csharp
Assert.Equal(csResult.Output, ssResult.Output);
```

---

## 📋 Test Cases Catalog

### 1. Fundamentals, Primitive Types & Operators

| Test Case | Motivation | Language Feature Under Test |
| :--- | :--- | :--- |
| `001_hello_world` | Ensure standard program entry point execution and console output. | Built-in `println` and `def main(): Unit` entry point. |
| `002_basic_arithmetic` | Validate operator precedence and associativity across integer operations. | Binary expressions with `+`, `-`, `*`, `/`, `%`. |
| `003_double_arithmetic` | Verify floating-point arithmetic and numeric type promotion (`Int` to `Double`). | Primitive `Double` and mixed arithmetic type compatibility. |
| `004_string_concatenation` | Validate string concatenation using the overloaded `+` operator. | String concatenation with string and non-string expressions. |
| `005_boolean_logic` | Test Boolean logic, relational expressions, and short-circuit evaluation. | Operators `&&`, `\|\|`, `==`, `!=`, `<`, `>`, `<=`, `>=`. |
| `006_unary_operators` | Ensure correct behavior of prefix unary operators. | Unary `-` (numeric negation) and `!` (logical negation). |

---

### 2. Control Flow & Expressions

| Test Case | Motivation | Language Feature Under Test |
| :--- | :--- | :--- |
| `007_if_expression` | Ensure that `if-else` operates as an **expression** that produces a value. | Expression `if (cond) expr1 else expr2` with return type inference. |
| `008_nested_if` | Verify cascading conditional branches and multi-condition evaluations. | Nested `if-else if-else` expressions. |
| `011_block_expression` | Allow scoped sequential declarations inside `{ ... }` where the final expression is returned. | Block expression `{ decls...; expr }` (IIFE / local block body). |
| `012_nested_blocks` | Test lexical scoping and hierarchical value computation inside nested blocks. | Nested block expressions with independent variable scopes. |

---

### 3. Declarations, Scoping & Lazy Evaluation

| Test Case | Motivation | Language Feature Under Test |
| :--- | :--- | :--- |
| `009_val_binding` | Test immutable identifier bindings with automatic type inference. | Immutable value declaration `val x = expr;`. |
| `010_val_type_annotation` | Validate type checking when explicit type annotations are specified by the author. | Type-annotated bindings `val x: Int = expr;`. |
| `013_lazy_val` | Support on-demand delayed evaluation and memoization for immutable values. | Lazy value modifier `lazy val x = expr;` (backed by `System.Lazy<T>`). |
| `014_lazy_val_side_effect` | Guarantee that side-effects in lazy initializers only execute upon first access. | Deferred evaluation semantics and side-effect memoization. |
| `015_scoped_variables` | Ensure variables declared inside blocks do not leak into outer scopes. | Lexical scope boundaries and symbol table isolation in `TypeChecker`. |
| `016_shadowing_val` | Enable variable shadowing in inner lexical scopes without mutating outer bindings. | Identifier shadowing in local nested scopes. |

---

### 4. Functions, Currying & Higher-Order Functions

| Test Case | Motivation | Language Feature Under Test |
| :--- | :--- | :--- |
| `017_def_simple_function` | Define and call first-order pure functions using concise expression body syntax. | Function definition `def name(params): Type = expr`. |
| `018_def_multi_param` | Validate multi-argument functions with heterogeneous parameter types. | Typed formal parameter lists. |
| `019_def_return_type` | Enforce strict return type verification between declared type and body expression. | Return type checking and validation in `TypeChecker`. |
| `020_curried_function` | Native support for curried functions with multiple parameter groups. | Curried definition `def f(a: Int)(b: Int): Int`. |
| `021_partial_application` | Enable partial application of curried functions to derive specialized functions. | Partial function application producing delegate closures. |
| `022_lambda_expression` | Treat functions as first-class citizens using anonymous lambda syntax. | Lambda expressions `(x: Int) => expr` inferred as `System.Func<...>`. |
| `023_higher_order_function` | Allow functions to accept other functions as arguments (`f: Int => Int`). | Function types in parameters (`A => B`) and lambda callbacks. |
| `024_function_returning_function` | Build higher-order function factories and closures capturing outer lexical context. | Functions returning functions (`Type => Type`) with closure captures. |

---

### 5. Algebraic Data Types (ADTs), Case Classes & Traits

| Test Case | Motivation | Language Feature Under Test |
| :--- | :--- | :--- |
| `025_simple_trait_and_class` | Define closed polymorphic type hierarchies via traits and case classes. | `sealed trait` and `case class ... extends Trait`. |
| `026_case_class_instantiation` | Instantiate immutable records and access positional constructor fields. | Positional case class instantiation `Point(x, y)` and field access `p.x`. |
| `027_case_object` | Model singleton instances within a type hierarchy (e.g. enum variants, empty states). | Declaration and access of `case object Name extends Trait`. |
| `028_adt_hierarchy` | Construct rich hierarchical data structures (trees, state machines, graphs). | Full ADT hierarchy with traits, case classes, and case objects. |
| `029_case_class_factory_call` | Invoke case class constructors as pure factory functions (without `new`). | Auto-generated static factory methods for case classes. |
| `030_case_class_equality` | Ensure structural value equality by default instead of reference equality. | Automatic structural equality in case classes (records). |

---

### 6. Pattern Matching

| Test Case | Motivation | Language Feature Under Test |
| :--- | :--- | :--- |
| `031_match_literal_int` | Branch on exact integer constants. | Constant pattern matching `case 1 => ...`. |
| `032_match_literal_string` | Branch on exact string literal constants. | Constant pattern matching `case "val" => ...`. |
| `033_match_wildcard` | Provide fallback default arms or ignore unused data components. | Wildcard pattern `case _ => ...`. |
| `034_match_identifier` | Match and capture any value by binding it to a local identifier. | Variable pattern `case x => ...` with local scope binding. |
| `035_match_constructor_single_param` | Deconstruct unary algebraic data types extracting the wrapped payload. | Constructor pattern `case Box(v) => ...`. |
| `036_match_constructor_multi_param` | Deconstruct multi-field algebraic data types extracting all positional elements. | N-ary constructor pattern `case Point(x, y) => ...`. |
| `037_match_nested_constructors` | Deconstruct deeply nested algebraic data structures in a single concise pattern. | Nested constructor patterns `case Node(Leaf(v), _) => ...`. |

---

### 7. Advanced Functional Programming (Haskell & Scala Patterns)

| Test Case | Motivation | Language Feature Under Test |
| :--- | :--- | :--- |
| `038_list_recursion_sum_and_len` | Implement the fundamental FP inductive list processing pattern. | Pattern matching with infix cons `head :: tail` and base case `Nil`. |
| `039_tailrec_accumulator` | Eliminate stack overflow risks in recursion-heavy functional algorithms. | `@tailrec` annotation and automatic transformation to efficient iterative loops. |
| `040_tree_algebraic_data_type` | Model and traverse non-linear recursive data structures (binary trees) purely functionally. | Inductive `Tree` ADT (`Leaf` / `Node`), recursive sum and maximum depth calculation. |
| `041_expression_evaluator` | Build an AST interpreter/symbolic calculator using typed ADTs and pattern-directed evaluation. | AST ADT (`Num`, `Add`, `Mul`, `Neg`) with recursive `eval` function. |
| `042_option_monad_safe_divide` | Prevent division-by-zero crashes and eliminate null references with option types. | Monadic `Option[T]` (`Some(v)` vs `None`), type covariance, and pattern matching. |
| `043_function_composition_and_pipeline` | Build complex data processing pipelines by chaining simple pure functions. | Higher-order functional combinators `compose(f, g)` and `andThen(f, g)`. |
| `044_lazy_call_by_name_short_circuit` | Create custom control flow constructs that only evaluate chosen branches. | Call-by-name parameters (`=> Type`) ensuring true short-circuiting of side-effects. |
| `045_peano_numbers_inductive_adt` | Formally model Peano arithmetic using inductive natural numbers. | Inductive Peano numbers (`Zero`, `Succ(n)`), recursive addition, and `Int` conversion. |
| `046_either_result_error_handling` | Robust error handling with strict types without throwing runtime exceptions. | Typed `Result` ADT (`Success(v)` / `Failure(err)`) with pipeline error propagation. |
| `047_higher_order_list_operations` | Standard immutable collection transformation idioms. | Functional list APIs: `List.map`, `List.filter`, `List.contains`, `List.size`. |

---

## ➕ Adding New Test Cases

1. Create a new directory inside `cases/` following the numerical ordering:
   ```text
   cases/
     └── 048_my_new_feature/
           ├── input.ss       <- SSharp source code
           └── expected.cs    <- Reference C# source with expected output
   ```
2. Run `dotnet test` — the test runner will automatically discover, compile, and verify the new case.
