def isEven(n: Int): Boolean = n % 2 == 0

def main(): Unit = {
    val nums = List(1, 2, 3, 4, 5, 6)
    val evens = nums.filter(isEven)
    val doubled = evens.map((x: Int) => x * 10)

    println(nums.size)
    println(evens.size)
    println(doubled.contains(20))
    println(doubled.contains(30))
}
