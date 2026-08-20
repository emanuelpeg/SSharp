# SSharp.Spec — Suite de Especificación y Tests del Lenguaje

La suite **SSharp.Spec** define la especificación formal y ejecutable del lenguaje **SSharp**. Cada caso de prueba valida el pipeline completo del compilador (`Lexer` → `Parser` → `TypeChecker` → `CodeGenerator` → `EvalBackend`), comparando la salida de ejecución del código fuente `.ss` contra un programa de referencia en C# `.cs`.

---

## 🚀 Cómo Ejecutar la Suite

Para correr todos los casos de especificación:

```powershell
dotnet test SSharp.Spec\SSharp.Spec.csproj
```

O para ver la salida detallada caso por caso:

```powershell
dotnet test SSharp.Spec\SSharp.Spec.csproj --logger "console;verbosity=normal"
```

---

## 🏗️ Estructura de un Caso de Prueba

Cada subdirectorio dentro de [`cases/`](cases/) contiene:
- `input.ss`: Código fuente en **SSharp** a evaluar.
- `expected.cs`: Código fuente de referencia en **C#** con la semántica y salida esperada.

El runner [`SpecTests.cs`](SpecTests.cs) compila y ejecuta ambos programas en memoria en contextos aislados y verifica que `ssResult.Output == csResult.Output`.

---

## 📋 Catálogo de Casos de Prueba

### 1. Fundamentos, Tipos Primitivos y Operadores

| Caso | Motivación | Característica Testeada |
| :--- | :--- | :--- |
| `001_hello_world` | Verificar el punto de entrada estándar y la función de impresión por consola. | Función built-in `println` y entry point `def main(): Unit`. |
| `002_basic_arithmetic` | Validar precedencia y asociatividad de operadores aritméticos enteros (`+`, `-`, `*`, `/`, `%`). | Expresiones binarias enteras y orden de operaciones. |
| `003_double_arithmetic` | Comprobar operaciones de punto flotante y promoción implícita de tipos numéricos (`Int` a `Double`). | Tipo primitivo `Double` y coherencia aritmética mixta. |
| `004_string_concatenation` | Validar la concatenación de cadenas de texto con valores de otros tipos mediante `+`. | Operador `+` sobrecargado para `String`. |
| `005_boolean_logic` | Probar evaluación lógica booleana con operadores de comparación y conjunción/disyunción. | Operadores `&&`, `\|\|`, `==`, `!=`, `<`, `>`. |
| `006_unary_operators` | Comprobar operadores unarios prefijos para inversión numérica y lógica. | Operadores unarios `-` (negación aritmética) y `!` (negación lógica). |

---

### 2. Control de Flujo y Expresiones

| Caso | Motivación | Característica Testeada |
| :--- | :--- | :--- |
| `007_if_expression` | Asegurar que `if-else` es una **expresión** que retorna un valor y no una sentencia. | Expresión `if (cond) expr1 else expr2` con inferencia de tipo. |
| `008_nested_if` | Probar composición y anidamiento de expresiones condicionales múltiples. | Expresiones `if-else if-else` anidadas en cascada. |
| `011_block_expression` | Permitir secuencias de declaraciones locales dentro de llaves `{ ... }` donde el último valor es el resultado. | Expresión de bloque `{ decls...; expr }` (traducción a IIFE o cuerpo local). |
| `012_nested_blocks` | Validar el anidamiento de bloques con resultados locales calculados jerárquicamente. | Bloques anidados con ámbitos léxicos independientes. |

---

### 3. Declaraciones, Variables, Ámbitos y Evaluación Perezosa

| Caso | Motivación | Característica Testeada |
| :--- | :--- | :--- |
| `009_val_binding` | Comprobar enlace inmutable de identificadores con inferencia de tipos. | Declaración inmutable `val x = expr;`. |
| `010_val_type_annotation` | Verificar consistencia cuando el desarrollador especifica explícitamente el tipo del valor. | Anotación de tipo en enlaces `val x: Int = expr;`. |
| `013_lazy_val` | Garantizar evaluación diferida (on-demand) y memorización de variables inmutables. | Modificador `lazy val x = expr;` (mapeado a `Lazy<T>`). |
| `014_lazy_val_side_effect` | Confirmar que los efectos secundarios de un `lazy val` no ocurren hasta su primer acceso. | Semántica de evaluación diferida y memoización de efectos colaterales. |
| `015_scoped_variables` | Validar que las variables declaradas dentro de un bloque no escapen a su ámbito léxico. | Reglas de visibilidad y scoping léxico en el TypeChecker. |
| `016_shadowing_val` | Permitir el ocultamiento (shadowing) de variables de ámbitos externos en ámbitos internos. | Shadowing de identificadores en scopes locales. |

---

### 4. Funciones, Currying y Orden Superior

| Caso | Motivación | Característica Testeada |
| :--- | :--- | :--- |
| `017_def_simple_function` | Definir y llamar funciones puras de primer orden con sintaxis concisa `= expr`. | Declaración `def name(params): Type = expr`. |
| `018_def_multi_param` | Comprobar funciones que reciben múltiples parámetros de diferentes tipos. | Listas de parámetros formales tipados. |
| `019_def_return_type` | Validar el chequeo estricto del tipo de retorno contra el cuerpo de la función. | Anotación y verificación de tipo de retorno en funciones. |
| `020_curried_function` | Implementar soporte nativo para funciones currificadas (`def f(x)(y)`). | Múltiples listas de parámetros `def f(a: Int)(b: Int): Int`. |
| `021_partial_application` | Permitir la aplicación parcial de funciones para reutilización y derivación funcional. | Aplicación parcial generando delegados de funciones remanentes. |
| `022_lambda_expression` | Tratar a las funciones como ciudadanos de primera clase mediante funciones anónimas. | Lambdas `(x: Int) => expr` con inferencia a `Func<...>`. |
| `023_higher_order_function` | Permitir funciones que reciben otras funciones como argumentos (`f: Int => Int`). | Tipos función en parámetros y pasaje de lambdas como callbacks. |
| `024_function_returning_function` | Construir fábricas de funciones y generadores (clausuras léxicas / closures). | Funciones que retornan funciones (`Type => Type`) y captura de entorno. |

---

### 5. Tipos Algebraicos de Datos (ADTs), Case Classes y Traits

| Caso | Motivación | Característica Testeada |
| :--- | :--- | :--- |
| `025_simple_trait_and_class` | Definir jerarquías polimórficas selladas mediante traits e interfaces funcionales. | `sealed trait` y `case class ... extends Trait`. |
| `026_case_class_instantiation` | Instanciar registros inmutables y acceder a sus propiedades posicionales. | Instanciación posicional `Point(x, y)` y acceso `p.x`. |
| `027_case_object` | Modelar valores únicos / singletons dentro de una jerarquía de tipos (e.g. enumeraciones). | Declaración y acceso a `case object Name extends Trait`. |
| `028_adt_hierarchy` | Construir estructuras jerárquicas complejas y heterogéneas (árboles, grafos, estados). | Jerarquía completa de ADT con traits, clases y objetos caso. |
| `029_case_class_factory_call` | Invocar constructores de case classes como funciones de fábrica puras (sin palabra clave `new`). | Métodos de fábrica estáticos autogenerados para case classes. |
| `030_case_class_equality` | Asegurar igualdad estructural por valor en lugar de igualdad por referencia. | Igualdad estructural automática en case classes (records). |

---

### 6. Pattern Matching

| Caso | Motivación | Característica Testeada |
| :--- | :--- | :--- |
| `031_match_literal_int` | Ramificar lógica según literales enteros exactos. | Pattern matching contra constantes enteras `case 1 => ...`. |
| `032_match_literal_string` | Ramificar lógica según literales de cadena exactos. | Pattern matching contra constantes de texto `case "val" => ...`. |
| `033_match_wildcard` | Manejar casos por defecto o ignorar partes irrelevantes del dato. | Patrón comodín `case _ => ...`. |
| `034_match_identifier` | Capturar cualquier valor ligándolo a un identificador dentro de la rama. | Patrón de variable `case x => ...` con enlace local. |
| `035_match_constructor_single_param` | Deconstruir tipos algebraicos unarios extrayendo su contenido encapsulado. | Patrón de constructor `case Box(v) => ...`. |
| `036_match_constructor_multi_param` | Deconstruir tipos algebraicos con múltiples componentes posicionales. | Patrón de constructor n-ario `case Point(x, y) => ...`. |
| `037_match_nested_constructors` | Deconstruir estructuras de datos profundamente anidadas en un solo paso sintáctico. | Patrones de constructores anidados `case Node(Leaf(v), _) => ...`. |

---

### 7. Paradigma Funcional Avanzado (Patrones Haskell & Scala)

| Caso | Motivación | Característica Testeada |
| :--- | :--- | :--- |
| `038_list_recursion_sum_and_len` | Implementar el patrón fundamental de procesamiento inductivo de listas en FP. | Pattern matching con constructor infijo `head :: tail` y caso base `Nil`. |
| `039_tailrec_accumulator` | Evitar desbordamiento de pila (Stack Overflow) en algoritmos recursivos intensivos. | Anotación `@tailrec` y transformación a bucles imperativos eficientes. |
| `040_tree_algebraic_data_type` | Modelar y recorrer estructuras de datos no lineales (árboles binarios) de forma recursiva pura. | ADT inductivo `Tree` (`Leaf` / `Node`), cálculo de suma total y profundidad. |
| `041_expression_evaluator` | Construir un intérprete/calculadora simbólica mediante AST tipado y evaluación pattern-directed. | ADT para AST aritmético (`Num`, `Add`, `Mul`, `Neg`) con función `eval`. |
| `042_option_monad_safe_divide` | Eliminar excepciones de división por cero y `NullReferenceException` usando tipos de opción. | Mónada `Option[T]` (`Some(v)` y `None`), propagación y tipado covariante. |
| `043_function_composition_and_pipeline` | Construir transformaciones complejas combinando funciones simples sin estado intermedio. | Operadores funcionales `compose(f, g)` y `andThen(f, g)`. |
| `044_lazy_call_by_name_short_circuit` | Implementar estructuras de control de flujo personalizadas que solo evalúen ramas necesarias. | Parámetros call-by-name (`=> Type`) garantizando cortocircuito de efectos secundarios. |
| `045_peano_numbers_inductive_adt` | Validar construcciones matemáticas formales mediante tipos inductivos de Peano. | Aritmética de Peano pura (`Zero`, `Succ(prev)`) y funciones de suma inductivas. |
| `046_either_result_error_handling` | Manejar errores con tipado estricto (`Result` / `Either`) transportando mensajes de fallo sin lanzar excepciones. | Patrón `Success(v)` / `Failure(err)` con propagación funcional en pipelines. |
| `047_higher_order_list_operations` | Emplear transformaciones estándar sobre colecciones inmutables. | Métodos funcionales de colección `List.map`, `List.filter`, `List.contains`, `List.size`. |

---

## ➕ Cómo Agregar Nuevos Casos

1. Crear un nuevo directorio dentro de `cases/` siguiendo la convención numérica:
   ```text
   cases/
     └── 048_mi_nuevo_caso/
           ├── input.ss       <- Código SSharp
           └── expected.cs    <- Código C# con el comportamiento/salida esperado
   ```
2. Ejecutar `dotnet test` para verificar que el caso compile y pase automáticamente (el runner los descubre dinámicamente).
