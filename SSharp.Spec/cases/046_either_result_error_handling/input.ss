sealed trait Result
case class Success(value: Int) extends Result
case class Failure(error: String) extends Result

def parsePositive(n: Int): Result =
    if (n > 0) Success(n) else Failure("Number must be positive")

def doubleResult(r: Result): Result = r match {
    case Success(v) => Success(v * 2)
    case Failure(err) => Failure(err)
}

def showResult(r: Result): String = r match {
    case Success(v) => "OK: " + v
    case Failure(err) => "ERR: " + err
}

def main(): Unit = {
    println(showResult(doubleResult(parsePositive(5))))
    println(showResult(doubleResult(parsePositive(0 - 3))))
}
