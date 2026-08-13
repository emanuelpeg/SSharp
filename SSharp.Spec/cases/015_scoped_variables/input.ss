def main(): Unit = {
    val x = 5
    val y = {
        val x = 100
        x + 1
    }
    println(x)
    println(y)
}
