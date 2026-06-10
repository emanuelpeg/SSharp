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

def main(): Unit = {
    val c = Circle(5.0)
    val r = Rectangle(4.0, 6.0)

    println("Circle area: " + area(c))
    println("Rectangle area: " + area(r))
    println("5! = " + factorial(5))
}