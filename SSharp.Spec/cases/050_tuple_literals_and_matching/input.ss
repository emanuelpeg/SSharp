val persona: (String, Int, Boolean) = ("Alice", 25, true)

def format(t: (String, Int)): String = t match {
    case (nombre, edad) => nombre + " tiene " + edad + " años"
}

def sum4(t: (Int, Int, Int, Int)): Int = t match {
    case (a, b, c, d) => a + b + c + d
}

def main(): Unit = {
    println(persona)
    println(persona._1)
    println(persona._2)
    println(persona._3)
    println(format(("Bob", 30)))
    println(sum4((1, 2, 3, 4)))
}
