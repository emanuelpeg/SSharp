def applyTwice(f: Int => Int, x: Int): Int = f(f(x))

def main(): Unit = {
    val inc = (x: Int) => x + 1
    println(applyTwice(inc, 5))
}
