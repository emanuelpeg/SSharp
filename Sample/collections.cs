using System;
using System.Collections.Generic;
using SSharp.Runtime;
using static SSharp.Runtime.Predef;

namespace SSharp.Generated;

public static class Program
{
    public static SSharp.Runtime.Unit main()
    {
        var l = List(1, 2, 3, 4, 5);
        println(("List original: " + l));
        println(("Size: " + l.Size));
        println(("Head: " + l.HeadValue));
        println(("Tail: " + l.TailList));
        println(("Reversed: " + l.Reverse()));
        println(("Take 3: " + l.Take(3)));
        println(("Drop 2: " + l.Drop(2)));
        println(("Contains 3: " + l.Contains(3)));
        println(("Contains 9: " + l.Contains(9)));
        println(("Forall > 0: " + l.Forall(new System.Func<int, bool>((x) => (x > 0)))));
        println(("Exists > 4: " + l.Exists(new System.Func<int, bool>((x) => (x > 4)))));
        var doubled = l.Map(new System.Func<int, int>((x) => (x * 2)));
        println(("Doubled: " + doubled));
        var evens = l.Filter(new System.Func<int, bool>((x) => ((x % 2) == 0)));
        println(("Evens: " + evens));
        var sum = l.FoldLeft(0, new System.Func<int, int, int>((acc, x) => (acc + x)));
        println(("Sum: " + sum));
        var appended = l.Appended(6);
        println(("Appended 6: " + appended));
        var prepended = l.Prepended(0);
        println(("Prepended 0: " + prepended));
        var l2 = List(6, 7, 8);
        var concatenated = l.Concat(l2);
        println(("Concat with [6,7,8]: " + concatenated));
        var s = Set<int>(3, 1, 2, 3, 1);
        println(("\\nSet (unique elements): " + s));
        println(("Set size: " + s.Size));
        println(("Contains 2: " + s.Contains(2)));
        println(("Contains 9: " + s.Contains(9)));
        var s2 = s.Incl(10);
        println(("After incl(10): " + s2));
        var s3 = s.Excl(2);
        println(("After excl(2): " + s3));
        var sA = Set<int>(1, 2, 3);
        var sB = Set<int>(2, 3, 4, 5);
        println(("Union: " + sA.Union(sB)));
        println(("Intersect: " + sA.Intersect(sB)));
        println(("Diff: " + sA.Diff(sB)));
        println(("ToList: " + s.ToList()));
        var m = Map<string, int>(Tuple2("uno", 1), Tuple2("dos", 2), Tuple2("tres", 3));
        println(("\\nMap: " + m));
        println(("Size: " + m.Size));
        println(("Contains 'dos': " + m.Contains("dos")));
        println(("Get 'uno': " + m.Get("uno")));
        println(("Get 'cuatro': " + m.Get("cuatro")));
        var m2 = m.Updated("cuatro", 4);
        println(("After updated('cuatro', 4): " + m2));
        var m3 = m.Removed("dos");
        println(("After removed('dos'): " + m3));
        println(("Keys: " + m.Keys()));
        println(("Values: " + m.Values()));
        return println(("ToList: " + m.ToList()));
    }
    public static void Main(string[] args)
    {
        main();
    }
}
