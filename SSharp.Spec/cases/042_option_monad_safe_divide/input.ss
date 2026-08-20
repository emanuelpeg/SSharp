def safeDivide(a: Int, b: Int): Option[Int] =
    if (b == 0) None else Some(a / b)

def showResult(opt: Option[Int]): String = opt match {
    case Some(v) => "Result: " + v
    case None => "Error: Division by zero"
}

def main(): Unit = {
    println(showResult(safeDivide(10, 2)))
    println(showResult(safeDivide(10, 0)))
}
