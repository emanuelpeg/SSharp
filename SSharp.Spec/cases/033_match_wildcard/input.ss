def isZero(x: Int): Boolean = x match {
    case 0 => true
    case _ => false
}

def main(): Unit = {
    println(isZero(0))
    println(isZero(10))
}
