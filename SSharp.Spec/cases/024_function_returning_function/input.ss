def makeMultiplier(factor: Int): Int => Int = (x: Int) => x * factor

def main(): Unit = {
    val triple = makeMultiplier(3)
    println(triple(7))
}
