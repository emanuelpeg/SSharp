def greet(name: String): String = name match {
    case "Alice" => "Hello Alice!"
    case "Bob"   => "Hello Bob!"
    case _       => "Hello stranger!"
}

def main(): Unit = {
    println(greet("Alice"))
    println(greet("Charlie"))
}
