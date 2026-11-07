using System;
using System.Collections.Generic;
using SSharp.Runtime;
using static SSharp.Runtime.Predef;

namespace SSharp.Generated;

public static class Program
{
    public static SSharp.Runtime.Unit main()
    {
        var s1 = Set<int>(1, 2, 3);
        var s2 = Set<int>(3, 4, 5);
        println(size(s1));
        println(isEmpty(s1));
        println(contains(2, s1));
        println(contains(9, s1));
        var sAdded = insert(10, s1);
        println(contains(10, sAdded));
        var sRemoved = delete(2, s1);
        println(contains(2, sRemoved));
        var sUnion = union(s1, s2);
        println(size(sUnion));
        var sInter = intersect(s1, s2);
        println(size(sInter));
        var sDiff = diff(s1, s2);
        println(size(sDiff));
        var m1 = Map<string, int>(new SSharp.Runtime.SSharpTuple2<string, int>("a", 1), new SSharp.Runtime.SSharpTuple2<string, int>("b", 2));
        println(size(m1));
        println(isEmpty(m1));
        println(contains("a", m1));
        println(contains("z", m1));
        var valA = get("a", m1);
        var valZ = get("z", m1);
        println(valA);
        println(valZ);
        var mAdded = insert("c", 3, m1);
        println(size(mAdded));
        var mDeleted = delete("a", m1);
        println(contains("a", mDeleted));
        var k = keys(m1);
        println(size(k));
        var v = values(m1);
        return println(length(v));
    }
    public static void Main(string[] args)
    {
        main();
    }
}
