using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace SSharp.Spec;

public class SpecTests
{
    private static readonly SpecRunner Runner = new();

    public static IEnumerable<object[]> GetTestCases()
    {
        string casesDir = FindCasesDirectory();
        if (!Directory.Exists(casesDir))
        {
            yield break;
        }

        var dirs = Directory.GetDirectories(casesDir).OrderBy(d => Path.GetFileName(d));
        foreach (var dir in dirs)
        {
            string caseName = Path.GetFileName(dir);
            string ssPath = Path.Combine(dir, "input.ss");
            string csPath = Path.Combine(dir, "expected.cs");

            if (File.Exists(ssPath) && File.Exists(csPath))
            {
                yield return new object[] { caseName, ssPath, csPath };
            }
        }
    }

    private static string FindCasesDirectory()
    {
        string? dir = Directory.GetCurrentDirectory();
        while (dir != null)
        {
            string candidate = Path.Combine(dir, "SSharp.Spec", "cases");
            if (Directory.Exists(candidate)) return candidate;

            candidate = Path.Combine(dir, "cases");
            if (Directory.Exists(candidate)) return candidate;

            dir = Directory.GetParent(dir)?.FullName;
        }

        dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            string candidate = Path.Combine(dir, "SSharp.Spec", "cases");
            if (Directory.Exists(candidate)) return candidate;

            candidate = Path.Combine(dir, "cases");
            if (Directory.Exists(candidate)) return candidate;

            dir = Directory.GetParent(dir)?.FullName;
        }

        return Path.Combine(Directory.GetCurrentDirectory(), "cases");
    }

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void TestSpecCase(string caseName, string ssPath, string csPath)
    {
        string ssCode = File.ReadAllText(ssPath);
        string csCode = File.ReadAllText(csPath);

        var ssResult = Runner.RunSSharp(ssCode);
        Assert.True(ssResult.Success, $"[{caseName}] SSharp execution failed:\n" + string.Join("\n", ssResult.Errors));

        var csResult = Runner.RunCSharp(csCode);
        Assert.True(csResult.Success, $"[{caseName}] C# reference execution failed:\n" + string.Join("\n", csResult.Errors));

        Assert.Equal(csResult.Output, ssResult.Output);
    }
}
