// Type aliases — sinónimos de tipos transparentes
type Predicate[A] = A => Boolean
type Transform[A, B] = A => B
type Point = (Double, Double)
type Name = String

def isEven(x: Int): Boolean = x % 2 == 0
def double(x: Int): Int = x * 2
def greet(name: String): String = "Hello, " + name

def applyPred(p: Predicate[Int], x: Int): Boolean = p(x)
def applyTransform(f: Transform[Int, Int], x: Int): Int = f(x)

def main(): Unit = {
    val evens: List[Int] = filter(isEven, List(1, 2, 3, 4, 5, 6))
    println(evens)

    val doubled: List[Int] = map(double, List(1, 2, 3))
    println(doubled)

    val msg: String = greet("World")
    println(msg)

    val label: Name = "origin"
    println(label)

    val r1: Boolean = applyPred(isEven, 4)
    val r2: Int = applyTransform(double, 7)
    println(r1)
    println(r2)
}
