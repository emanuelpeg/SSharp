data Shape = Circle(radius: Double) | Rectangle(width: Double, height: Double) | Dot

def area(s: Shape): Double = s match {
    case Circle(r) => 3.14 * r * r
    case Rectangle(w, h) => w * h
    case Dot => 0.0
}

def main(): Unit = {
    val c: Shape = Circle(10.0)
    val r: Shape = Rectangle(4.0, 5.0)
    val d: Shape = Dot

    // Pattern matching over native ADT
    println(area(c))
    println(area(r))
    println(area(d))

    // Pure field selector functions generated automatically: radius(s), width(s), height(s)
    println(radius(c))
    println(width(r))
    println(height(r))
}
