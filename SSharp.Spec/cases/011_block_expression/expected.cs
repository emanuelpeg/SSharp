using System;

public static class Program
{
    public static void Main()
    {
        int res = new Func<int>(() => {
            int x = 10;
            int y = 20;
            return x + y;
        })();
        Console.WriteLine(res);
    }
}
