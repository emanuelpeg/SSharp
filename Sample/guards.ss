def evaluar(n: Int): String = n match {
    case x if x < 0 => "Negativo"
    case 0 => "Cero"
    case x if x % 2 == 0 => "Positivo par"
    case _ => "Positivo impar"
}

// As-Patterns (@) para ligar el todo y sus partes:
def duplicateHead(l: List[Int]): List[Int] = l match {
    case Nil => Nil
    case original @ (h :: _) => h :: original
}

def main(): Unit = {
    println(evaluar(-5))
    println(evaluar(0))
    println(evaluar(4))
    println(evaluar(7))
    println(duplicateHead(List(1, 2, 3)))
}
