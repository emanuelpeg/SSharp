// 3. Tuplas Literales N-arias (Tuple2 - Tuple8)
// Sintaxis literal para empaquetar datos heterogéneos sin crear clases nominales

val persona: (String, Int, Boolean) = ("Alice", 25, true)

// Deconstrucción en funciones / pattern matching:
def format(t: (String, Int)): String = t match {
    case (nombre, edad) => nombre + " tiene " + edad + " años"
}

def sumTuple4(t: (Int, Int, Int, Int)): Int = t match {
    case (a, b, c, d) => a + b + c + d
}

def main(): Unit = {
    // Imprimir tupla completa
    println("persona: " + persona)

    // Acceso a campos posicionales (_1, _2, _3)
    println("Nombre: " + persona._1)
    println("Edad: " + persona._2)
    println("Activo: " + persona._3)

    // Pattern matching con tuplas
    val info = ("Bob", 30)
    println(format(info))
    println(format(("Charlie", 40)))

    // Tupla de aridad 4
    val t4: (Int, Int, Int, Int) = (10, 20, 30, 40)
    println("Suma t4 (10, 20, 30, 40): " + sumTuple4(t4))
}
