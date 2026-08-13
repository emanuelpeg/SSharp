sealed trait Tree
case class Leaf(value: Int) extends Tree
case class Node(left: Tree, right: Tree) extends Tree

def main(): Unit = {
    val t = Node(Leaf(1), Leaf(2))
    println(t != null)
}
