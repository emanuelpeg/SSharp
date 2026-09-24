// 2. Composición y Flujo con Operador Pipe (|>)
// Para encadenar transformaciones de datos sin anidamiento excesivo ni métodos OO:

def isEven(n: Int): Boolean = n % 2 == 0

def doubleIt(n: Int): Int = n * 2

def add(a: Int, b: Int): Int = a + b

def main(): Unit = {
    val xs = List(1, 2, 3, 4, 5, 6, 7, 8, 9, 10)

    // Pipeline de transformaciones sobre listas
    val resultado = xs
        |> filter(isEven)
        |> map(doubleIt)
        |> take(5)

    println("xs: " + xs)
    println("resultado: " + resultado)

    // Pipeline con reducción (sum)
    val total = xs
        |> filter(isEven)
        |> map(doubleIt)
        |> sum

    println("total suma: " + total)

    // Pipeline con funciones escalares
    val calc = 5
        |> doubleIt
        |> add(10)

    println("5 |> doubleIt |> add(10) = " + calc)
}
