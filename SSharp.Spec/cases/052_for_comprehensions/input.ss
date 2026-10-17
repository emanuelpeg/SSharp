def main(): Unit = {
    val pares = for {
        x <- List(1, 2, 3)
        y <- List(10, 20)
        if x + y > 15
    } yield x + y
    println(pares)

    val single = for (x <- List(1, 2, 3)) yield x * 2
    println(single)

    val opt1: Option[Int] = Some(5)
    val opt2: Option[Int] = Some(15)
    val optNone: Option[Int] = None
    val sumSome = for {
        a <- opt1
        b <- opt2
    } yield a + b
    println(sumSome)

    val sumNone = for {
        a <- opt1
        b <- optNone
    } yield a + b
    println(sumNone)
}
