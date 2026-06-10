using System;
using System.IO;
using System.Linq;
using SSharp.Backend;
using SSharp.Compiler;

namespace SSharp.CLI;

public static class Program
{
    private static void PrintUsage()
    {
        Console.WriteLine("Usage: SSharp.CLI <input-file.ss> [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -o <output-file>     Output path for the generated .cs file");
        Console.WriteLine("                       (defaults to <input-file>.cs)");
        Console.WriteLine("  -c, --compile        Compile the generated C# to a .NET executable (.dll)");
        Console.WriteLine("                       (defaults to <input-file>.dll)");
        Console.WriteLine("  --out-dll <path>     Output path for the compiled .dll");
        Console.WriteLine("                       (only used with -c / --compile)");
        Console.WriteLine("  --runtime-dll <path> Explicit path to SSharp.Runtime.dll");
        Console.WriteLine("                       (only used with -c / --compile)");
    }

    public static int Main(string[] args)
    {
        if (args.Length < 1 || args[0] is "-h" or "--help")
        {
            PrintUsage();
            return args.Length < 1 ? 1 : 0;
        }

        string inputFile    = args[0];
        string outputCs     = "";
        string outputDll    = "";
        string runtimeDll   = "";
        bool   doCompile    = false;

        for (int i = 1; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-o" when i + 1 < args.Length:
                    outputCs = args[++i];
                    break;

                case "-c":
                case "--compile":
                    doCompile = true;
                    break;

                case "--out-dll" when i + 1 < args.Length:
                    outputDll = args[++i];
                    break;

                case "--runtime-dll" when i + 1 < args.Length:
                    runtimeDll = args[++i];
                    break;

                default:
                    Console.Error.WriteLine($"Unknown option: {args[i]}");
                    PrintUsage();
                    return 1;
            }
        }

        if (string.IsNullOrEmpty(outputCs))
            outputCs = Path.ChangeExtension(inputFile, ".cs");

        if (string.IsNullOrEmpty(outputDll))
            outputDll = Path.ChangeExtension(inputFile, ".dll");

        if (!File.Exists(inputFile))
        {
            Console.Error.WriteLine($"Error: Input file '{inputFile}' does not exist.");
            return 1;
        }

        try
        {
            string sourceCode = File.ReadAllText(inputFile);

            // ── 1. Lexing ────────────────────────────────────────────────────
            var lexer  = new Lexer(sourceCode);
            var tokens = lexer.ScanTokens();

            var lexerErrors = tokens.Where(t => t.Type == TokenType.Error).ToList();
            if (lexerErrors.Any())
            {
                Console.Error.WriteLine("Lexer errors:");
                foreach (var err in lexerErrors)
                    Console.Error.WriteLine($"  [{err.Line}:{err.Column}] {err.Value}");
                return 1;
            }

            // ── 2. Parsing ───────────────────────────────────────────────────
            var parser = new Parser(tokens);
            var ast    = parser.ParseProgram();

            if (parser.Errors.Any())
            {
                Console.Error.WriteLine("Parser errors:");
                foreach (var err in parser.Errors)
                    Console.Error.WriteLine($"  {err}");
                return 1;
            }

            // ── 3. Type Checking ─────────────────────────────────────────────
            var typeChecker = new TypeChecker();
            typeChecker.Check(ast);

            if (typeChecker.Errors.Any())
            {
                Console.Error.WriteLine("Type errors:");
                foreach (var err in typeChecker.Errors)
                    Console.Error.WriteLine($"  {err}");
                return 1;
            }

            // ── 4. Code Generation ───────────────────────────────────────────
            var codeGen       = new CodeGenerator(typeChecker.ResolvedTypes);
            string generatedCs = codeGen.Generate(ast);

            File.WriteAllText(outputCs, generatedCs);
            Console.WriteLine($"  [1/2] C# source  → {outputCs}");

            // ── 5. (Optional) Roslyn Compilation ────────────────────────────
            if (!doCompile)
            {
                Console.WriteLine("Done. (Use -c to also compile to a .NET executable.)");
                return 0;
            }

            Console.WriteLine($"  [2/2] Compiling  → {outputDll}");

            var backend = new CSharpBackend();
            var result  = backend.Compile(
                generatedCs,
                outputDll,
                string.IsNullOrEmpty(runtimeDll) ? null : runtimeDll);

            if (!result.Success)
            {
                Console.Error.WriteLine("Roslyn compilation errors:");
                foreach (var err in result.Errors)
                    Console.Error.WriteLine($"  {err}");
                return 1;
            }

            Console.WriteLine($"Done. Run with: dotnet {outputDll}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }
}
