sealed trait Nat
case object Zero extends Nat
case class Succ(prev: Nat) extends Nat

def toInt(n: Nat): Int = n match {
    case Zero => 0
    case Succ(p) => 1 + toInt(p)
}

def addNat(a: Nat, b: Nat): Nat = a match {
    case Zero => b
    case Succ(p) => Succ(addNat(p, b))
}

def main(): Unit = {
    val two = Succ(Succ(Zero))
    val three = Succ(Succ(Succ(Zero)))
    val five = addNat(two, three)

    println(toInt(two))
    println(toInt(three))
    println(toInt(five))
}
