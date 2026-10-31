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
        var lCons = (((new SSharp.Runtime.Nil<int>()).Prepended(3)).Prepended(2)).Prepended(1);
        println(("List creada con 1::2::3::Nil: " + lCons));
        println(("Size: " + length(l)));
        println(("Head: " + head(l)));
        println(("Tail: " + tail(l)));
        println(("Reversed: " + reverse(l)));
        println(("Take 3: " + take(3, l)));
        println(("Drop 2: " + drop(2, l)));
        println(("Contains 3: " + contains(3, l)));
        println(("Contains 9: " + contains(9, l)));
        println(("Forall > 0: " + forall(new System.Func<int, bool>((x) => (x > 0)), l)));
        println(("Exists > 4: " + exists(new System.Func<int, bool>((x) => (x > 4)), l)));
        var doubled = map(new System.Func<int, int>((x) => (x * 2)), l);
        println(("Doubled: " + doubled));
        var evens = filter(new System.Func<int, bool>((x) => ((x % 2) == 0)), l);
        println(("Evens: " + evens));
        var sum = foldLeft(0, new System.Func<int, int, int>((acc, x) => (acc + x)), l);
        println(("Sum: " + sum));
        var appended = append(6, l);
        println(("Appended 6: " + appended));
        var prepended = prepend(0, l);
        println(("Prepended 0: " + prepended));
        var l2 = List(6, 7, 8);
        var concatenated = concat(l, l2);
        println(("Concat with [6,7,8]: " + concatenated));
        var s = Set<int>(3, 1, 2, 3, 1);
        println(("\\nSet (unique elements): " + s));
        println(("Set size: " + size(s)));
        println(("Contains 2: " + contains(2, s)));
        println(("Contains 9: " + contains(9, s)));
        var s2 = insert(10, s);
        println(("After insert(10): " + s2));
        var s3 = delete(2, s);
        println(("After delete(2): " + s3));
        var sA = Set<int>(1, 2, 3);
        var sB = Set<int>(2, 3, 4, 5);
        println(("Union: " + union(sA, sB)));
        println(("Intersect: " + intersect(sA, sB)));
        println(("Diff: " + diff(sA, sB)));
        println(("ToList: " + toList(s)));
        var m = Map<string, int>(Tuple2("uno", 1), Tuple2("dos", 2), Tuple2("tres", 3));
        println(("\\nMap: " + m));
        println(("Size: " + size(m)));
        println(("Contains 'dos': " + contains("dos", m)));
        println(("Get 'uno': " + get("uno", m)));
        println(("Get 'cuatro': " + get("cuatro", m)));
        var m2 = insert("cuatro", 4, m);
        println(("After insert('cuatro', 4): " + m2));
        var m3 = delete("dos", m);
        println(("After delete('dos'): " + m3));
        println(("Keys: " + keys(m)));
        println(("Values: " + values(m)));
        return println(("ToList: " + toList(m)));
    }
    public static void Main(string[] args)
    {
        main();
    }
}
