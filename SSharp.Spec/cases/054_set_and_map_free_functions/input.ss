def main(): Unit = {
    // ── Set free functions ──
    val s1: Set[Int] = Set(1, 2, 3)
    val s2: Set[Int] = Set(3, 4, 5)

    println(size(s1))
    println(isEmpty(s1))
    println(contains(2, s1))
    println(contains(9, s1))

    val sAdded = insert(10, s1)
    println(contains(10, sAdded))

    val sRemoved = delete(2, s1)
    println(contains(2, sRemoved))

    val sUnion = union(s1, s2)
    println(size(sUnion))

    val sInter = intersect(s1, s2)
    println(size(sInter))

    val sDiff = diff(s1, s2)
    println(size(sDiff))

    // ── Map free functions ──
    val m1: Map[String, Int] = Map(("a", 1), ("b", 2))

    println(size(m1))
    println(isEmpty(m1))
    println(contains("a", m1))
    println(contains("z", m1))

    val valA: Option[Int] = get("a", m1)
    val valZ: Option[Int] = get("z", m1)
    println(valA)
    println(valZ)

    val mAdded = insert("c", 3, m1)
    println(size(mAdded))

    val mDeleted = delete("a", m1)
    println(contains("a", mDeleted))

    val k = keys(m1)
    println(size(k))

    val v = values(m1)
    println(length(v))
}
