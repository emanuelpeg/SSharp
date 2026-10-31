def main(): Unit = {
    // ── Option Monad (funciones libres - Haskell: Maybe) ─────────────────────
    val opt1 = Option(42)
    val opt2: Option[Int] = None
    println("Option(42): " + opt1)
    println("Option(None): " + opt2)
    println("isEmpty: " + isEmpty(opt1))
    println("isDefined: " + isDefined(opt1))
    println("getOrElse: " + getOrElse(99, opt2))

    // flatMap sobre Option (Haskell: >>= o bind)
    val mappedOpt = flatMap((x: Int) => Option(x * 2), opt1)
    println("Option(42) flatMap (* 2): " + mappedOpt)

    // flatten sobre Option (Haskell: join)
    val nestedOpt = Option(Option(7))
    println("Option(Option(7)): " + nestedOpt)
    println("Flattened: " + flatten(nestedOpt))

    // ── List Monad ───────────────────────────────────────────────────────────
    val l = List(1, 2, 3)
    // flatMap (Haskell: concatMap)
    val flatMappedList = flatMap((x: Int) => List(x, x * 10), l)
    println("\nList(1, 2, 3) flatMap: " + flatMappedList)

    // flatten (Haskell: concat)
    val nestedList = List(List(1, 2), List(3, 4))
    println("nestedList: " + nestedList)
    println("Flattened list: " + flatten(nestedList))

    // ── Set Monad ────────────────────────────────────────────────────────────
    val s = Set(1, 2)
    // flatMap
    val flatMappedSet = flatMap((x: Int) => Set(x, x * 100), s)
    println("\nSet(1, 2) flatMap: " + flatMappedSet)

    // flatten
    val nestedSet = Set(Set(1, 2), Set(2, 3))
    println("nestedSet: " + nestedSet)
    println("Flattened set: " + flatten(nestedSet))

    // ── Map Monad ────────────────────────────────────────────────────────────
    val m = Map(Tuple2("a", 1), Tuple2("b", 2))
    // flatMap con tuplas procesadas funcionalmente con fst y snd
    val flatMappedMap = flatMap((t: (String, Int)) => Map(Tuple2(fst(t), snd(t) * 10), Tuple2(fst(t) + "!", snd(t) * 100)), m)
    println("\nMap(a->1, b->2) flatMap: " + flatMappedMap)
}
