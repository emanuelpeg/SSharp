def boom(): Int = {
    println("BOOM")
    0
}

def myIf(cond: Boolean, thenBranch: => Int, elseBranch: => Int): Int =
    if (cond) thenBranch else elseBranch

def main(): Unit = {
    val a = myIf(true, 42, boom())
    println(a)

    val b = myIf(false, boom(), 99)
    println(b)
}
