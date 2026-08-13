def check(x: Int): String = x match {
    case 1 => "one"
    case 2 => "two"
    case _ => "other"
}

def main(): Unit = {
    println(check(1))
    println(check(2))
    println(check(5))
}
