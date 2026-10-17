// 5. For-Comprehensions Desugareadas a Funciones Libres
// Las comprensiones monádicas se desugarean directamente a llamadas a las funciones libres flatMap, map y filter:

def main(): Unit = {
    // Ejemplo básico: múltiples generadores y filtro con guard 'if'
    val pares = for {
        x <- List(1, 2, 3)
        y <- List(10, 20)
        if x + y > 15
    } yield x + y

    println("pares: " + pares)

    // Ejemplo con tuplas en el generador
    val combinaciones = for {
        n <- List(1, 2, 3)
        if n % 2 != 0
        s <- List("a", "b")
    } yield (n, s)

    println("combinaciones: " + combinaciones)

    // Ejemplo monádico con Option
    val optA: Option[Int] = Some(10)
    val optB: Option[Int] = Some(20)
    val sumaOpt = for {
        a <- optA
        b <- optB
    } yield a + b

    println("sumaOpt: " + sumaOpt)
}
