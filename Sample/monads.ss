def main(): Unit = {
    // ── Option Monad ─────────────────────────────────────────────────────────
    val opt1 = Option(42)
    val opt2 = Option(null)
    println("Option(42): " + opt1)
    println("Option(null): " + opt2)
    println("isEmpty: " + opt1.isEmpty)
    println("isDefined: " + opt1.isDefined)
    println("getOrElse: " + opt2.getOrElse(() => 99))

    // Test flatMap on Option
    val mappedOpt = opt1.flatMap((x: Int) => Option(x * 2))
    println("Option(42) flatMap (* 2): " + mappedOpt)

    // Test flatten on Option
    val nestedOpt = Option(Option(7))
    println("Option(Option(7)): " + nestedOpt)
    println("Flattened: " + nestedOpt.flatten())

    // ── List Monad ───────────────────────────────────────────────────────────
    val l = List(1, 2, 3)
    // flatMap
    val flatMappedList = l.flatMap((x: Int) => List(x, x * 10))
    println("\nList(1, 2, 3) flatMap: " + flatMappedList)

    // flatten
    val nestedList = List(List(1, 2), List(3, 4))
    println("nestedList: " + nestedList)
    println("Flattened list: " + nestedList.flatten())

    // ── Set Monad ────────────────────────────────────────────────────────────
    val s = Set(1, 2)
    // flatMap
    val flatMappedSet = s.flatMap((x: Int) => Set(x, x * 100))
    println("\nSet(1, 2) flatMap: " + flatMappedSet)

    // flatten
    val nestedSet = Set(Set(1, 2), Set(2, 3))
    println("nestedSet: " + nestedSet)
    println("Flattened set: " + nestedSet.flatten())

    // ── Map Monad ────────────────────────────────────────────────────────────
    val m = Map(Tuple2("a", 1), Tuple2("b", 2))
    // flatMap
    val flatMappedMap = m.flatMap((t: Tuple2[String, Int]) => Map(Tuple2(t._1, t._2 * 10), Tuple2(t._1 + "!", t._2 * 100)))
    println("\nMap(a->1, b->2) flatMap: " + flatMappedMap)
}
