def sum(l: List[Int]): Int = l match {
    case Nil => 0
    case h :: t => h + sum(t)
}

def len(l: List[Int]): Int = l match {
    case Nil => 0
    case _ :: t => 1 + len(t)
}

def main(): Unit = {
    val nums = List(10, 20, 30, 40)
    println(sum(nums))
    println(len(nums))
}
