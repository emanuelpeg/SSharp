sealed trait Shape
case class Circle(radius: Double) extends Shape
case class Rectangle(width: Double, height: Double) extends Shape

def area(s: Shape): Double = s match {
    case Circle(r)       => 3.14159 * r * r
    case Rectangle(w, h) => w * h
}

def factorial(n: Int): Int =
    if (n <= 1) 1 else n * factorial(n - 1)

@tailrec
def len[T](l: List[T])(default: Int): Int = l match {
    case Nil => default
    case head::tail => len(tail, default + 1)
}

def lenDef[T](l: List[T]): Int = len(l, 0)

def sum(x: Int)(y: Int): Int = x + y

// Evaluación perezosa por nombre (Call-by-name) pura:
// En Haskell todas las expresiones son perezosas por defecto.
// Aquí se demuestra sin efectos secundarios impuros: si la condición es falsa,
// la expresión perezosa no se evalúa (evitando división por cero en runtime).
def evaluateIf(condition: Boolean, expression: => Int): Int =
    if (condition) expression else 0

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
    println("head(l) = " + head(l))
    println("tail(l) = " + tail(l))
    println("add 2 to 3 = " + add2(3))
    val resultAdd2 = if (add2(10) == 12) "Yes" else "No" 
    println("add 2 to 10 = " + resultAdd2) 

    println("Testing pure lazy parameters (call-by-name):")
    // Con condición falsa, la división por cero no se evalúa en runtime
    val zero = 0
    val lazyZero = evaluateIf(false, 100 / zero)
    println("lazyZero (safe without evaluating): " + lazyZero)
    val lazyVal = evaluateIf(true, 10 * 5)
    println("lazyVal (evaluated when true): " + lazyVal)
}