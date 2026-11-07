def safeDivide(a: Int, b: Int): Either[String, Int] = {
    if (b == 0) Left("division by zero")
    else Right(a / b)
}

def main(): Unit = {
    val ok: Either[String, Int] = safeDivide(10, 2)
    val err: Either[String, Int] = safeDivide(10, 0)

    // isLeft / isRight
    println(isRight(ok))
    println(isLeft(ok))
    println(isRight(err))
    println(isLeft(err))

    // getOrElse
    println(getOrElse(0, ok))
    println(getOrElse(0, err))

    // map / mapEither (maps Right value)
    val mappedOk = mapEither((x: Int) => x * 10, ok)
    val mappedErr = mapEither((x: Int) => x * 10, err)
    println(mappedOk)
    println(mappedErr)

    // mapLeft (maps Left value)
    val leftMapped = mapLeft((msg: String) => "Error: " + msg, err)
    println(leftMapped)

    // pattern matching on Left and Right
    val resOk: String = ok match {
        case Right(v) => "Success: " + v
        case Left(e) => "Fail: " + e
    }
    val resErr: String = err match {
        case Right(v) => "Success: " + v
        case Left(e) => "Fail: " + e
    }
    println(resOk)
    println(resErr)
}
