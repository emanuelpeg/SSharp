# Guía Completa de la Sintaxis de SSharp

**SSharp** es un lenguaje de programación funcional, estáticamente tipado y basado en expresiones que transpila a C# ejecutable sobre la plataforma .NET. Inspirado en el subconjunto funcional de Scala, combina la expresividad del paradigma funcional con la interoperabilidad y el rendimiento de la runtime de .NET.

---

## Tabla de Contenidos

1. [Sistema de Tipos](#1-sistema-de-tipos)
2. [Valores y Enlaces (`val` / `lazy val`)](#2-valores-y-enlaces-val--lazy-val)
3. [Funciones (`def`)](#3-funciones-def)
4. [Expresiones Lambda](#4-expresiones-lambda)
5. [Estructuras de Control y Bloques](#5-estructuras-de-control-y-bloques)
6. [Tipos de Datos Algebraicos (ADTs)](#6-tipos-de-datos-algebraicos-adts)
7. [Pattern Matching (`match` / `case`)](#7-pattern-matching-match--case)
8. [Listas y Colecciones](#8-listas-y-colecciones)
9. [Biblioteca de Runtime (`SSharp.Runtime`)](#9-biblioteca-de-runtime-ssharprunetime)
10. [Comentarios e Importaciones](#10-comentarios-e-importaciones)
11. [Ejemplo Integrador Completo](#11-ejemplo-integrador-completo)
12. [Herramientas CLI y REPL](#12-herramientas-cli-y-repl)

---

## 1. Sistema de Tipos

SSharp posee un sistema de tipos estático con inferencia de tipos automática.

### Tipos Primitivos

| Tipo SSharp | Equivalente C# | Descripción | Ejemplo de Literal |
|-------------|----------------|-------------|--------------------|
| `Int` | `int` | Entero de 32 bits con signo | `42`, `-10` |
| `Double` | `double` | Punto flotante de 64 bits | `3.14159`, `-0.5` |
| `String` | `string` | Cadena de caracteres | `"Hola mundo"` |
| `Boolean` | `bool` | Valor booleano | `true`, `false` |
| `Unit` | `SSharp.Runtime.Unit` | Tipo con un único valor `()` (representa la ausencia de valor devuelto) | `()` |
| `Any` | `object` | Tipo raíz de la jerarquía de tipos | Cualquiera |

### Tipos Compuestos y Genéricos

- **Listas**: `List[A]`
- **Opciones**: `Option[A]`
- **Conjuntos**: `Set[A]`
- **Mapas**: `Map[K, V]`
- **Tuplas**: `Tuple2[A, B]`
- **Funciones**: `(A, B) => C` (función que toma argumentos de tipo `A` y `B` y retorna `C`)

---

## 2. Valores y Enlaces (`val` / `lazy val`)

En SSharp los enlaces son inmutables por defecto.

### Enlaces Inmutables (`val`)

Se declaran con la palabra clave `val`. El tipo puede ser explícito o inferido:

```scala
// Con tipo inferido automáticamente
val x = 42
val mensaje = "Hola SSharp"

// Con tipo explícito
val pi: Double = 3.14159265359
val activo: Boolean = true
```

### Enlaces Perezosos / Memoizados (`lazy val`)

Un valor declarado con `lazy val` se evalúa únicamente la primera vez que es accedido. El resultado se almacena en memoria (memoización) y se reutiliza en subsiguientes accesos:

```scala
lazy val computacionCostosa: Int = {
    println("Calculando resultado complejo...")
    10 * 20 * 30
}

println("Antes del acceso")
println(computacionCostosa) // Imprime "Calculando resultado complejo..." y luego "6000"
println(computacionCostosa) // Imprime únicamente "6000" (se usa el valor almacenado)
```

---

## 3. Funciones (`def`)

Las funciones se definen utilizando la palabra clave `def`.

### Sintaxis Básica

```scala
def sumar(a: Int, b: Int): Int = a + b

def saludar(nombre: String): String = "Hola, " + nombre
```

### Funciones con Bloques de Código

Si el cuerpo de la función requiere múltiples sentencias, se encierra en un bloque `{ }`. El valor de la última expresión es retornado automáticamente:

```scala
def calcularAreaTriangulo(base: Double, altura: Double): Double = {
    val area = (base * altura) / 2.0
    area
}
```

### Funciones Genéricas

Las funciones pueden aceptar parámetros de tipo genérico encerrados entre corchetes `[T]`:

```scala
def identidad[T](x: T): T = x

def primero[A, B](a: A, b: B): A = a
```

### Listas de Parámetros Múltiples (Currying) y Aplicación Parcial

SSharp permite definir funciones con múltiples listas de parámetros. Esto facilita la aplicación parcial de funciones:

```scala
// Función currificada
def sumarCurry(x: Int)(y: Int): Int = x + y

// Aplicación parcial (retorna una función de tipo Int => Int)
val sumarCinco = sumarCurry(5)

val resultado = sumarCinco(10) // 15
```

### Parámetros por Nombre (Lazy Parameters)

Un parámetro con la notación `=> T` se evalúa cada vez que es referenciado dentro del cuerpo de la función (call-by-name):

```scala
def evaluarSiVerdadero(condicion: Boolean, expresion: => Int): Int = {
    if (condicion) expresion else 0
}
```

### Recursión y Optimización `@tailrec`

Para funciones recursivas puras, la anotación `@tailrec` instruye al compilador a verificar que la llamada recursiva esté en posición de cola (tail call). El compilador la optimizará transformándola en un bucle imperativo en C# libre de desbordamiento de pila (stack overflow):

```scala
@tailrec
def factorialTailrec(n: Int, acumulador: Int): Int = {
    if (n <= 1) acumulador
    else factorialTailrec(n - 1, n * acumulador)
}

def factorial(n: Int): Int = factorialTailrec(n, 1)
```

---

## 4. Expresiones Lambda

Las funciones anónimas o lambdas se definen usando la flecha `=>`:

```scala
// Lambda simple
val duplicar = (x: Int) => x * 2

// Lambda con múltiples parámetros
val multiplicar = (x: Int, y: Int) => x * y

// Pasar una lambda a una función de orden superior
val lista = List(1, 2, 3, 4)
val duplicados = lista.map((x: Int) => x * 2)
```

---

## 5. Estructuras de Control y Bloques

SSharp es un lenguaje basado en expresiones: **todo en SSharp produce un valor**.

### Expresión `if` / `else`

La estructura `if` siempre debe incluir una rama `else` y devuelve el valor de la rama ejecutada:

```scala
val x = 10
val signo: String = if (x >= 0) "Positivo" else "Negativo"

val maximo = if (a > b) a else b
```

### Bloques de Expresión (`{ }`)

Un bloque de código compuesto por sentencias entre llaves devuelve el valor de su última expresión:

```scala
val resultado: Int = {
    val a = 10
    val b = 20
    a + b // Valor de retorno del bloque: 30
}
```

### Operadores Operativos

- **Aritméticos**: `+`, `-`, `*`, `/`, `%`
- **Relacionales**: `==`, `!=`, `<`, `<=`, `>`, `>=`
- **Lógicos**: `&&`, `||`, `!`

---

## 6. Tipos de Datos Algebraicos (ADTs)

SSharp soporta Tipos de Datos Algebraicos mediante `sealed trait`, `case class` y `case object`.

### Traits Sellados (`sealed trait`)

Define la interfaz base o tipo algebraico raíz. Todos sus subtipos deben definirse en el mismo archivo:

```scala
sealed trait Figura
```

### Clases de Caso (`case class`)

Representan constructores de datos con campos inmutables. Automáticamente generan métodos para comparación por valor, `toString` estructurado y desestructuración en pattern matching:

```scala
case class Circulo(radio: Double) extends Figura
case class Rectangulo(ancho: Double, alto: Double) extends Figura
```

### Objetos de Caso (`case object`)

Representan valores singleton o casos sin parámetros (por ejemplo, el caso vacio o un estado constante):

```scala
case object FiguraVacia extends Figura
```

### ADTs Genéricos

Los traits y case classes pueden ser genéricos:

```scala
sealed trait Opcion[A]
case class Alguno[A](valor: A) extends Opcion[A]
case class Ninguno[A]() extends Opcion[A]
```

---

## 7. Pattern Matching (`match` / `case`)

El patrón matching inspecciona estructuras de datos y evalúa la rama que coincida.

### Sintaxis Básica

```scala
def describirFigura(f: Figura): String = f match {
    case Circulo(r)          => "Círculo de radio " + r
    case Rectangulo(w, h)    => "Rectángulo de " + w + "x" + h
    case FiguraVacia         => "Figura vacía"
}
```

### Tipos de Patrones Soportados

1. **Patrón Comodín (`_`)**: Coincide con cualquier valor sin enlazarlo a un nombre.
2. **Patrón Literal**: Coincide con valores concretos (`42`, `"hola"`, `true`).
3. **Patrón de Identificador / Variable**: Captura el valor en una variable local.
4. **Patrón de Constructor**: Desestructura una `case class`.
5. **Patrón Infijo de Lista (`head :: tail`)**: Separa la cabeza y la cola de una lista.

```scala
def procesar(x: Any): String = x match {
    case 0          => "Es el número cero"
    case "admin"    => "Es el usuario administrador"
    case n: Int     => "Es un entero: " + n
    case _          => "Valor desconocido"
}
```

---

## 8. Listas y Colecciones

SSharp proporciona estructuras de datos inmutables y persistentes.

### Construcción de Listas

Las listas pueden construirse con la fábrica `List(...)` o mediante el operador infijo de inserción al inicio `::` (cons) y el objeto `Nil`:

```scala
import "SSharp.Runtime"

// Construcción con fábrica
val l1 = List(1, 2, 3)

// Construcción con cons (::) y Nil (asociativo a la derecha)
val l2 = 1 :: 2 :: 3 :: Nil
```

### Pattern Matching en Listas

```scala
def sumarElementos(lista: List[Int]): Int = lista match {
    case Nil        => 0
    case head::tail => head + sumarElementos(tail)
}
```

---

## 9. Biblioteca de Runtime (`SSharp.Runtime`)

Para hacer uso del runtime estándar de SSharp, importa el módulo:

```scala
import "SSharp.Runtime"
```

### Funciones de E/S Incluidas

- `print(x: Any): Unit`: Imprime en la consola sin salto de línea.
- `println(x: Any): Unit`: Imprime en la consola agregando un salto de línea.
- `readLine(): String`: Lee una línea de texto introducida por el usuario en la consola.

### Métodos de Orden Superior en Colecciones

Las listas y colecciones inmutables en SSharp incluyen los métodos funcionales estándar:

```scala
val numeros = List(1, 2, 3, 4, 5)

// map: Transforma cada elemento
val dobles = numeros.map((x: Int) => x * 2)

// filter: Filtra los elementos según un predicado
val pares = numeros.filter((x: Int) => x % 2 == 0)

// foldLeft: Reduce la lista acumulando un valor desde la izquierda
val sumaTotal = numeros.foldLeft(0, (acc: Int, x: Int) => acc + x)
```

---

## 10. Comentarios e Importaciones

### Comentarios

```scala
// Esto es un comentario de una sola línea

/*
   Esto es un comentario
   multilínea
*/
```

### Importaciones

Para importar espacios de nombres o módulos de .NET / SSharp:

```scala
import "SSharp.Runtime"
```

---

## 11. Ejemplo Integrador Completo

```scala
import "SSharp.Runtime"

// Definición de jerarquía de datos (ADT)
sealed trait Arbol[A]
case class Hoja[A](valor: A) extends Arbol[A]
case class Nodo[A](izq: Arbol[A], der: Arbol[A]) extends Arbol[A]

// Función recursiva con pattern matching
def contarHojas[A](arbol: Arbol[A]): Int = arbol match {
    case Hoja(_)     => 1
    case Nodo(i, d)  => contarHojas(i) + contarHojas(d)
}

// Función currificada
def multiplicarPor(factor: Int)(numero: Int): Int = factor * numero

def main(): Unit = {
    val miArbol = Nodo(Hoja(10), Nodo(Hoja(20), Hoja(30)))
    val totalHojas = contarHojas(miArbol)

    val porDiez = multiplicarPor(10)
    val resultado = porDiez(5)

    println("Total de hojas en el árbol: " + totalHojas)
    println("10 * 5 = " + resultado)
}
```

---

## 12. Herramientas CLI y REPL

### Uso del Compilador CLI (`SSharp.CLI`)

```sh
# Transpilar un archivo .ss a C#
dotnet run --project SSharp.CLI -- programa.ss

# Transpilar y compilar a executable .dll de .NET
dotnet run --project SSharp.CLI -- programa.ss -c

# Transpilar, compilar y ejecutar inmediatamente
dotnet run --project SSharp.CLI -- programa.ss -r
```

### Uso del REPL Interactivo

Para iniciar la consola interactiva:

```sh
dotnet run --project SSharp.CLI -- repl
```

Comandos útiles dentro del REPL:
- `:help` / `:h` : Muestra el menú de ayuda.
- `:quit` / `:q` : Abandona la sesión interactiva.
- `:reset`      : Reinicia el contexto acumulado de la sesión.
- `:ctx`        : Muestra el código fuente acumulado en el contexto actual.
