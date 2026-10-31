// 3. Tuplas Literales N-arias (Tuple2 - Tuple8)
// Sintaxis funcional para empaquetar datos heterogéneos sin clases nominales

val persona: (String, Int, Boolean) = ("Alice", 25, true)

// Deconstrucción funcional pura mediante Pattern Matching:
def describePersona(p: (String, Int, Boolean)): String = p match {
    case (nombre, edad, activo) => nombre + " (edad: " + edad + ", activo: " + activo + ")"
}

def format(t: (String, Int)): String = t match {
    case (nombre, edad) => nombre + " tiene " + edad + " años"
}

def sumTuple4(t: (Int, Int, Int, Int)): Int = t match {
    case (a, b, c, d) => a + b + c + d
}

def main(): Unit = {
    // Imprimir tupla completa
    println("persona: " + persona)

    // Deconstrucción funcional completa con pattern matching
    println(describePersona(persona))

    // Funciones selectoras puras para pares (Haskell: fst, snd)
    val par = ("Bob", 30)
    println("fst(par): " + fst(par))
    println("snd(par): " + snd(par))

    // Pattern matching con tuplas
    println(format(par))
    println(format(("Charlie", 40)))

    // Tupla de aridad 4
    val t4: (Int, Int, Int, Int) = (10, 20, 30, 40)
    println("Suma t4 (10, 20, 30, 40): " + sumTuple4(t4))
}
