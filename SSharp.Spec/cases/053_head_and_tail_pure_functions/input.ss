def main(): Unit = {
    val xs: List[Int] = List(1, 2, 3, 4, 5)

    // head y tail (funciones libres puras)
    val h: Int = head(xs)
    val t: List[Int] = tail(xs)
    println(h)
    println(t)

    // last e init (Haskell: last / init)
    val l: Int = last(xs)
    val i: List[Int] = init(xs)
    println(l)
    println(i)

    // headOption — safe (Haskell: listToMaybe)
    val empty: List[Int] = List(0) |> tail
    val hSome: Option[Int] = headOption(xs)
    val hNone: Option[Int] = headOption(empty)
    println(hSome)
    println(hNone)

    // tailOption — safe
    val tSome: Option[List[Int]] = tailOption(xs)
    val tNone: Option[List[Int]] = tailOption(empty)
    println(tSome)
    println(tNone)

    // uncons — deconstrucción segura (Haskell: uncons)
    val u: Option[(Int, List[Int])] = uncons(xs)
    val uNone: Option[(Int, List[Int])] = uncons(empty)
    println(u)
    println(uNone)
}
