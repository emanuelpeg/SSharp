sealed trait Expr
case class Num(value: Int) extends Expr
case class Add(lhs: Expr, rhs: Expr) extends Expr
case class Mul(lhs: Expr, rhs: Expr) extends Expr
case class Neg(expr: Expr) extends Expr

def eval(e: Expr): Int = e match {
    case Num(v) => v
    case Add(l, r) => eval(l) + eval(r)
    case Mul(l, r) => eval(l) * eval(r)
    case Neg(x) => 0 - eval(x)
}

def main(): Unit = {
    val expr = Add(Mul(Num(3), Num(4)), Neg(Num(2)))
    println(eval(expr))
}
