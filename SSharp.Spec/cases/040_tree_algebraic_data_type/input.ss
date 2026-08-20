sealed trait Tree
case class Leaf(value: Int) extends Tree
case class Node(left: Tree, right: Tree) extends Tree

def sumTree(t: Tree): Int = t match {
    case Leaf(v) => v
    case Node(l, r) => sumTree(l) + sumTree(r)
}

def max(a: Int, b: Int): Int = if (a > b) a else b

def depth(t: Tree): Int = t match {
    case Leaf(_) => 1
    case Node(l, r) => 1 + max(depth(l), depth(r))
}

def main(): Unit = {
    val tree = Node(Node(Leaf(1), Leaf(2)), Leaf(3))
    println(sumTree(tree))
    println(depth(tree))
}
