def isEven(n: Int): Boolean = n % 2 == 0

def double(x: Int): Int = x * 2

def main(): Unit = {
    val nums = List(1, 2, 3, 4, 5, 6)

    val len     = length(nums)
    val evens   = filter(isEven, nums)
    val doubled = map(double, evens)
    val total   = foldLeft(0, (acc: Int, x: Int) => acc + x, doubled)
    val rev     = reverse(evens)
    val taken   = take(2, nums)

    println(len)
    println(evens)
    println(doubled)
    println(total)
    println(rev)
    println(taken)
    println(contains(4, nums))
    println(isEmpty(nums))
}
