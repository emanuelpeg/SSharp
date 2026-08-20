@tailrec
def factorial(n: Int, acc: Int): Int =
    if (n <= 1) acc else factorial(n - 1, acc * n)

@tailrec
def fib(n: Int, a: Int, b: Int): Int =
    if (n == 0) a
    else if (n == 1) b
    else fib(n - 1, b, a + b)

def main(): Unit = {
    println(factorial(5, 1))
    println(factorial(6, 1))
    println(fib(6, 0, 1))
    println(fib(7, 0, 1))
}
