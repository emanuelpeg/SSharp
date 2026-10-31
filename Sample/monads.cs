using System;
using System.Collections.Generic;
using SSharp.Runtime;
using static SSharp.Runtime.Predef;

namespace SSharp.Generated;

public static class Program
{
    public static SSharp.Runtime.Unit main()
    {
        var opt1 = Option<int>(42);
        var opt2 = new SSharp.Runtime.None<int>();
        println(("Option(42): " + opt1));
        println(("Option(None): " + opt2));
        println(("isEmpty: " + isEmpty(opt1)));
        println(("isDefined: " + isDefined(opt1)));
        println(("getOrElse: " + getOrElse(99, opt2)));
        var mappedOpt = flatMap(new System.Func<int, SSharp.Runtime.SSharpOption<int>>((x) => Option<int>((x * 2))), opt1);
        println(("Option(42) flatMap (* 2): " + mappedOpt));
        var nestedOpt = Option<SSharp.Runtime.SSharpOption<int>>(Option<int>(7));
        println(("Option(Option(7)): " + nestedOpt));
        println(("Flattened: " + flatten(nestedOpt)));
        var l = List(1, 2, 3);
        var flatMappedList = flatMap(new System.Func<int, SSharp.Runtime.SSharpList<int>>((x) => List(x, (x * 10))), l);
        println(("\\nList(1, 2, 3) flatMap: " + flatMappedList));
        var nestedList = List(List(1, 2), List(3, 4));
        println(("nestedList: " + nestedList));
        println(("Flattened list: " + flatten(nestedList)));
        var s = Set<int>(1, 2);
        var flatMappedSet = flatMap(new System.Func<int, SSharp.Runtime.SSharpSet<int>>((x) => Set<int>(x, (x * 100))), s);
        println(("\\nSet(1, 2) flatMap: " + flatMappedSet));
        var nestedSet = Set<SSharp.Runtime.SSharpSet<int>>(Set<int>(1, 2), Set<int>(2, 3));
        println(("nestedSet: " + nestedSet));
        println(("Flattened set: " + flatten(nestedSet)));
        var m = Map<string, int>(Tuple2("a", 1), Tuple2("b", 2));
        var flatMappedMap = flatMap(new System.Func<SSharp.Runtime.SSharpTuple2<string, int>, SSharp.Runtime.SSharpMap<string, int>>((t) => Map<string, int>(Tuple2(fst(t), (snd(t) * 10)), Tuple2((fst(t) + "!"), (snd(t) * 100)))), m);
        return println(("\\nMap(a->1, b->2) flatMap: " + flatMappedMap));
    }
    public static void Main(string[] args)
    {
        main();
    }
}
