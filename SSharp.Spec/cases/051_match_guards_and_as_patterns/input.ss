// Guards: case pattern if condition => body
def evaluar(n: Int): String = n match {
    case x if x < 0 => "Negativo"
    case 0 => "Cero"
    case x if x % 2 == 0 => "Positivo par"
    case _ => "Positivo impar"
}

// As-patterns: name @ subPattern
// Bind the whole Cons cell and also its head
def describeFirst(l: List[Int]): String = l match {
    case original @ (h :: t) => "first=" + h + " size=+" + h
    case Nil => "empty"
}

def main(): Unit = {
    println(evaluar(-5))
    println(evaluar(0))
    println(evaluar(4))
    println(evaluar(7))
    println(describeFirst(List(1, 2, 3)))
}
