def main(): Unit = {
    val score = 85
    val grade = if (score >= 90) "A" else if (score >= 80) "B" else "C"
    println(grade)
}
