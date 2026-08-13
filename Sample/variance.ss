// Demostración de covarianza (+T) y contravarianza (-T) en SSharp
// Sintaxis idéntica a Scala

// --- Covarianza (+T) ---
// Una lista covariant: si Cat <: Animal, entonces List[Cat] <: List[Animal]
sealed trait Animal
case class Cat(name: String) extends Animal
case class Dog(name: String) extends Animal

// Trait covariante: si B <: A, entonces Container[B] <: Container[A]
trait Container[+A]
case class Box(value: String) extends Container[String]

// --- Contravarianza (-T) ---
// Un "consumer" contravariante: si Animal <: Cat, entonces Consumer[Cat] <: Consumer[Animal]
// (un consumidor más general puede ser usado donde se espera uno más específico)
trait Printer[-A]

// --- Invarianza (T) ---
// Mutable por naturaleza: si A != B, entonces MutableBox[A] no es subtipo de MutableBox[B]
trait MutableBox[A]

// --- Demostración de pattern matching con traits como interfaces ---
sealed trait Shape
case class Circle(radius: Double) extends Shape
case class Rectangle(width: Double, height: Double) extends Shape

def area(s: Shape): Double = s match {
    case Circle(r) => 3.14 * r * r
    case Rectangle(w, h) => w * h
}

val c = Circle(5.0)
val r = Rectangle(4.0, 6.0)

println(area(c))
println(area(r))
