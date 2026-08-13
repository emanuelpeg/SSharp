def describe(x: Int): String = x match {
    case n => "number is " + n
}

def main(): Unit = {
    println(describe(42))
}
