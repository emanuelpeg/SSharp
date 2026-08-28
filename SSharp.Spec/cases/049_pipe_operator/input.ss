def isEven(n: Int): Boolean = n % 2 == 0

def double(n: Int): Int = n * 2

def add(a: Int, b: Int): Int = a + b

def main(): Unit = {
    val res1 = List(1, 2, 3, 4, 5, 6)
        |> filter(isEven)
        |> map(double)
        |> sum

    val res2 = 5
        |> double
        |> add(10)

    val res3 = List(10, 20, 30)
        |> length

    val res4 = List(1, 2, 3)
        |> map((x: Int) => x * 10)

    println(res1)
    println(res2)
    println(res3)
    println(res4)
}
