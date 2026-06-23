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


@tailrec
def len[T](l: List[T])(default : Int): Int = l match {
    case Nil => default
    case head::tail => len(tail, default + 1)
}

def lenDef[T](l: List[T]): Int = len(l, 0)

def sum(x: Int)(y: Int): Int = x + y

def and(a: Boolean, b: => Boolean): Boolean =
    if (a) b else false

def sideEffect(): Boolean = {
    println("Side effect executed!")
    true
}

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
    val resultAdd2 = if (add2(10) == 12) "Yes" else "No" 
    println("add 2 to 10= " + resultAdd2) 

    println("Testing lazy params:")
    val lazyRes1 = and(false, sideEffect())
    println("lazyRes1 (should not execute side effect): " + lazyRes1)
    val lazyRes2 = and(true, sideEffect())
    println("lazyRes2 (should execute side effect): " + lazyRes2)
}