using System;
using System.IO;
using System.Linq;
using SSharp.Compiler;

namespace SSharp.CLI;

public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: SSharp.CLI <input-file> [-o <output-file>]");
            return 1;
        }

        string inputFile = args[0];
        string outputFile = "";

        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "-o" && i + 1 < args.Length)
            {
                outputFile = args[i + 1];
                i++;
            }
        }

        if (string.IsNullOrEmpty(outputFile))
        {
            outputFile = Path.ChangeExtension(inputFile, ".cs");
        }

        if (!File.Exists(inputFile))
        {
            Console.Error.WriteLine($"Error: Input file '{inputFile}' does not exist.");
            return 1;
        }

        try
        {
            string sourceCode = File.ReadAllText(inputFile);

            // 1. Lexing
            var lexer = new Lexer(sourceCode);
            var tokens = lexer.ScanTokens();

            var lexerErrors = tokens.Where(t => t.Type == TokenType.Error).ToList();
            if (lexerErrors.Any())
            {
                Console.Error.WriteLine("Lexer errors found:");
                foreach (var err in lexerErrors)
                {
                    Console.Error.WriteLine($"[{err.Line}:{err.Column}] Error: {err.Value}");
                }
                return 1;
            }

            // 2. Parsing
            var parser = new Parser(tokens);
            var ast = parser.ParseProgram();

            if (parser.Errors.Any())
            {
                Console.Error.WriteLine("Parser errors found:");
                foreach (var err in parser.Errors)
                {
                    Console.Error.WriteLine(err);
                }
                return 1;
            }

            // 3. Type Checking
            var typeChecker = new TypeChecker();
            typeChecker.Check(ast);

            if (typeChecker.Errors.Any())
            {
                Console.Error.WriteLine("Type Checker errors found:");
                foreach (var err in typeChecker.Errors)
                {
                    Console.Error.WriteLine(err);
                }
                return 1;
            }

            // 4. Code Generation
            var codeGenerator = new CodeGenerator(typeChecker.ResolvedTypes);
            string generatedCode = codeGenerator.Generate(ast);

            // 5. Output
            File.WriteAllText(outputFile, generatedCode);
            Console.WriteLine($"Compilation successful. Generated C# code written to '{outputFile}'.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected compilation error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }
}
