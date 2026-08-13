sealed trait OptionVal
case class SomeVal(v: Int) extends OptionVal
case object NoneVal extends OptionVal

def getOrDefault(opt: OptionVal): Int = opt match {
    case SomeVal(x) => x
    case NoneVal    => 0
}

def main(): Unit = {
    println(getOrDefault(SomeVal(10)))
    println(getOrDefault(NoneVal))
}
