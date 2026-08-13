sealed trait Expr
case class Num(n: Int) extends Expr
case class Add(e1: Expr, e2: Expr) extends Expr

def eval(e: Expr): Int = e match {
    case Num(n) => n
    case Add(e1, e2) => eval(e1) + eval(e2)
}

def main(): Unit = {
    val expr = Add(Num(10), Add(Num(20), Num(30)))
    println(eval(expr))
}
