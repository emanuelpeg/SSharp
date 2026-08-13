lazy val message: String = {
    println("evaluated")
    "hello"
}

def main(): Unit = {
    println("before")
    println(message)
    println(message)
}
