def main(): Unit = {
    // ── Option Inspection: isSome / isNone / isDefined / isEmpty ──
    val s: Option[Int] = Some(42)
    val n: Option[Int] = None

    println(isSome(s))
    println(isNone(s))
    println(isDefined(s))
    println(isEmpty(s))

    println(isSome(n))
    println(isNone(n))
    println(isDefined(n))
    println(isEmpty(n))

    // ── getOrElse ──
    println(getOrElse(0, s))
    println(getOrElse(0, n))

    // ── Option Transformations: map & filter ──
    val mappedSome = map((x: Int) => x * 2, s)
    val mappedNone = map((x: Int) => x * 2, n)
    println(mappedSome)
    println(mappedNone)

    val filteredKeep = filter((x: Int) => x > 40, s)
    val filteredDrop = filter((x: Int) => x > 50, s)
    println(filteredKeep)
    println(filteredDrop)

    // ── flatten ──
    val nestedSome: Option[Option[Int]] = Some(Some(99))
    val nestedNone: Option[Option[Int]] = Some(None)
    println(flatten(nestedSome))
    println(flatten(nestedNone))
}
