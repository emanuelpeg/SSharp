case class Person(name: String, age: Int)

def isAdult(p: Person): Boolean = p match {
    case Person(name, age) => age >= 18
}

def main(): Unit = {
    println(isAdult(Person("Bob", 20)))
    println(isAdult(Person("Kid", 10)))
}
