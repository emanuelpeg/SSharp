def isEven(n: Int): Boolean = n % 2 == 0

def main(): Unit = {
    val nums = List(1, 2, 3, 4, 5, 6)
    val evens = filter(isEven, nums)
    val doubled = map((x: Int) => x * 10, evens)

    println(size(nums))
    println(size(evens))
    println(contains(20, doubled))
    println(contains(30, doubled))
}
