def compose(f: Int => Int, g: Int => Int): Int => Int =
    (x: Int) => f(g(x))

def andThen(f: Int => Int, g: Int => Int): Int => Int =
    (x: Int) => g(f(x))

def main(): Unit = {
    val doubleIt = (x: Int) => x * 2
    val addTen = (x: Int) => x + 10

    val f1 = compose(addTen, doubleIt)
    val f2 = andThen(addTen, doubleIt)

    println(f1(5))
    println(f2(5))
}
