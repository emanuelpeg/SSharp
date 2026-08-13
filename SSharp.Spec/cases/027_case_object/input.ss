sealed trait Color
case object Red extends Color

def main(): Unit = {
    val c = Red
    println(c != null)
}
