using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace SSharp.Backend;

/// <summary>Result of a Roslyn compilation.</summary>
public record CompilationResult(bool Success, IReadOnlyList<string> Errors);

/// <summary>
/// Compiles a C# source string to a .NET executable (.dll) using Roslyn.
/// Automatically discovers all trusted platform assemblies and SSharp.Runtime.
/// </summary>
public class CSharpBackend
{
    /// <summary>
    /// Compiles <paramref name="csharpSource"/> and writes the output assembly to
    /// <paramref name="outputDllPath"/>. A <c>.runtimeconfig.json</c> is written
    /// alongside so the output can be run with <c>dotnet outputDllPath</c>.
    /// </summary>
    /// <param name="csharpSource">Full C# source text to compile.</param>
    /// <param name="outputDllPath">Target path for the output .dll file.</param>
    /// <param name="runtimeDllPath">
    ///   Optional explicit path to SSharp.Runtime.dll.
    ///   When null, the backend auto-discovers it next to itself.
    /// </param>
    public CompilationResult Compile(
        string csharpSource,
        string outputDllPath,
        string? runtimeDllPath = null)
    {
        // 1. Parse source
        var syntaxTree = CSharpSyntaxTree.ParseText(
            csharpSource,
            new CSharpParseOptions(LanguageVersion.Latest));

        // 2. Build metadata references
        //    Start from every trusted platform assembly provided by the host runtime.
        var references = new List<MetadataReference>();

        var tpaData = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? "";
        foreach (var asmPath in tpaData.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            if (File.Exists(asmPath))
            {
                references.Add(MetadataReference.CreateFromFile(asmPath));
            }
        }

        // 3. Add SSharp.Runtime.dll reference
        string? runtimePath = ResolveRuntimeDll(runtimeDllPath);
        if (runtimePath != null)
        {
            references.Add(MetadataReference.CreateFromFile(runtimePath));
        }
        else
        {
            return new CompilationResult(false, new[]
            {
                "SSharp.Runtime.dll could not be located. " +
                "Pass its path explicitly via --runtime-dll."
            });
        }

        // 4. Create compilation
        var compilation = CSharpCompilation.Create(
            assemblyName: Path.GetFileNameWithoutExtension(outputDllPath),
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication)
                .WithOptimizationLevel(OptimizationLevel.Release)
                .WithNullableContextOptions(NullableContextOptions.Enable)
                .WithPlatform(Platform.AnyCpu));

        // 5. Emit
        string? tempPath = null;
        try
        {
            tempPath = outputDllPath + ".tmp";
            EmitResult emitResult;

            using (var stream = File.Create(tempPath))
            {
                emitResult = compilation.Emit(stream);
            }

            if (!emitResult.Success)
            {
                File.Delete(tempPath);

                var errors = emitResult.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => d.ToString())
                    .ToList();

                return new CompilationResult(false, errors);
            }

            // Atomically replace output
            if (File.Exists(outputDllPath)) File.Delete(outputDllPath);
            File.Move(tempPath, outputDllPath);
        }
        catch
        {
            if (tempPath != null && File.Exists(tempPath)) File.Delete(tempPath);
            throw;
        }

        // 6. Write .runtimeconfig.json
        WriteRuntimeConfig(outputDllPath);

        // 7. Copy SSharp.Runtime.dll next to the output so it can be resolved at runtime
        string outputDir = Path.GetDirectoryName(outputDllPath) ?? ".";
        string runtimeDest = Path.Combine(outputDir, "SSharp.Runtime.dll");
        if (!string.Equals(Path.GetFullPath(runtimePath), Path.GetFullPath(runtimeDest), StringComparison.OrdinalIgnoreCase))
        {
            File.Copy(runtimePath, runtimeDest, overwrite: true);
        }

        return new CompilationResult(true, Array.Empty<string>());
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string? ResolveRuntimeDll(string? explicitPath)
    {
        if (explicitPath != null && File.Exists(explicitPath))
            return explicitPath;

        // Look next to the SSharp.Backend assembly
        string? asmDir = Path.GetDirectoryName(
            typeof(CSharpBackend).Assembly.Location);

        if (asmDir != null)
        {
            string candidate = Path.Combine(asmDir, "SSharp.Runtime.dll");
            if (File.Exists(candidate)) return candidate;
        }

        // Look next to the entry-point assembly
        string? entryDir = Path.GetDirectoryName(
            Assembly.GetEntryAssembly()?.Location ?? "");

        if (entryDir != null)
        {
            string candidate = Path.Combine(entryDir, "SSharp.Runtime.dll");
            if (File.Exists(candidate)) return candidate;
        }

        return null;
    }

    private static void WriteRuntimeConfig(string dllPath)
    {
        // Detect the running .NET major version so the generated config is accurate.
        string tfm = $"net{Environment.Version.Major}.0";

        string json = $$"""
            {
              "runtimeOptions": {
                "tfm": "{{tfm}}",
                "framework": {
                  "name": "Microsoft.NETCore.App",
                  "version": "{{Environment.Version.Major}}.0.0"
                }
              }
            }
            """;

        File.WriteAllText(Path.ChangeExtension(dllPath, ".runtimeconfig.json"), json);
    }
}
