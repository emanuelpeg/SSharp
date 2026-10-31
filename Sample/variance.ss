// Polimorfismo Funcional Puro y Tipos de Datos Algebraicos (ADTs)
// Referencia canónica: Haskell (data Shape = Circle Float | Rectangle Float Float)
// En Haskell no hay subtipado (A <: B) ni varianza OO (+A/-A); el polimorfismo es paramétrico puro.

// --- Jerarquía de Datos Algebraicos (ADTs: Sum Types) ---
sealed trait Animal
case class Cat(name: String) extends Animal
case class Dog(name: String) extends Animal

// Función pura de procesamiento de datos por Pattern Matching
def animalSound(a: Animal): String = a match {
    case Cat(n) => n + " dice: Miau"
    case Dog(n) => n + " dice: Guau"
}

// --- Contenedor Paramétrico Puro (Polimorfismo Paramétrico) ---
// En programación funcional pura, los contenedores son inmutables y genéricos:
sealed trait Container[A]
case class Box[A](value: A) extends Container[A]

def unbox[A](b: Container[A]): A = b match {
    case Box(v) => v
}

// --- Tipo de Datos Algebraico para Formas Geométricas ---
sealed trait Shape
case class Circle(radius: Double) extends Shape
case class Rectangle(width: Double, height: Double) extends Shape

def area(s: Shape): Double = s match {
    case Circle(r)       => 3.14159265 * r * r
    case Rectangle(w, h) => w * h
}

def main(): Unit = {
    val michi: Animal = Cat("Michi")
    val firulais: Animal = Dog("Firulais")
    println(animalSound(michi))
    println(animalSound(firulais))

    val intBox: Container[Int] = Box(42)
    val strBox: Container[String] = Box("SSharp Funcional")
    println("Box int: " + unbox(intBox))
    println("Box str: " + unbox(strBox))

    val c: Shape = Circle(5.0)
    val r: Shape = Rectangle(4.0, 6.0)
    println("Área círculo: " + area(c))
    println("Área rectángulo: " + area(r))
}
