using System;
using System.Collections.Generic;
using SSharp.Runtime;
using static SSharp.Runtime.Predef;

namespace SSharp.Generated;

public static class Program
{
    public static SSharp.Runtime.SSharpEither<string, int> safeDivide(int a, int b)
    {
        return ((b == 0) ? Left<string, int>("division by zero") : Right<string, int>((a / b)));
    }
    public static SSharp.Runtime.Unit main()
    {
        var ok = safeDivide(10, 2);
        var err = safeDivide(10, 0);
        println(isRight(ok));
        println(isLeft(ok));
        println(isRight(err));
        println(isLeft(err));
        println(getOrElse(0, ok));
        println(getOrElse(0, err));
        var mappedOk = mapEither(new System.Func<int, int>((x) => (x * 10)), ok);
        var mappedErr = mapEither(new System.Func<int, int>((x) => (x * 10)), err);
        println(mappedOk);
        println(mappedErr);
        var leftMapped = mapLeft(new System.Func<string, string>((msg) => ("Error: " + msg)), err);
        println(leftMapped);
        var resOk = (ok) switch
{
    SSharp.Runtime.Right<string, int>(var v) => ("Success: " + v),
    SSharp.Runtime.Left<string, int>(var e) => ("Fail: " + e),
    _ => throw new System.InvalidOperationException("Pattern match failed")
};
        var resErr = (err) switch
{
    SSharp.Runtime.Right<string, int>(var v) => ("Success: " + v),
    SSharp.Runtime.Left<string, int>(var e) => ("Fail: " + e),
    _ => throw new System.InvalidOperationException("Pattern match failed")
};
        println(resOk);
        return println(resErr);
    }
    public static void Main(string[] args)
    {
        main();
    }
}
