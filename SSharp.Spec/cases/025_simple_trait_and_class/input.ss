sealed trait Person
case class Student(name: String, age: Int) extends Person

def main(): Unit = {
    val s = Student("Alice", 20)
    println(s.name)
    println(s.age)
}
