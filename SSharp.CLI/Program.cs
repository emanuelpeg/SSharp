using System;
using System.IO;
using System.Linq;
using SSharp.Backend;
using SSharp.Compiler;

namespace SSharp.CLI;

public static class Program
{
    private static bool TryParseError(string kind, string errStr, out string? filePath, out int line, out int col, out string cleanMsg)
    {
        filePath = null;
        line = 1;
        col = 1;
        cleanMsg = errStr;

        // Format 1: [line:col] ...
        if (errStr.StartsWith('['))
        {
            int closeBracket = errStr.IndexOf(']');
            if (closeBracket > 0)
            {
                string locPart = errStr.Substring(1, closeBracket - 1);
                int colon = locPart.IndexOf(':');
                if (colon > 0)
                {
                    if (int.TryParse(locPart.Substring(0, colon), out int l) &&
                        int.TryParse(locPart.Substring(colon + 1), out int c))
                    {
                        line = l;
                        col = c;
                        cleanMsg = errStr.Substring(closeBracket + 1).Trim();
                        return true;
                    }
                }
            }
        }

        // Format 2: filepath(line,col): error CODE: message
        // Example: "c:\path\file.cs(12,34): error CS0103: ..."
        int openParen = errStr.IndexOf('(');
        int closeParen = errStr.IndexOf(')');
        if (openParen > 0 && closeParen > openParen)
        {
            string locPart = errStr.Substring(openParen + 1, closeParen - openParen - 1);
            int comma = locPart.IndexOf(',');
            if (comma > 0)
            {
                if (int.TryParse(locPart.Substring(0, comma), out int l) &&
                    int.TryParse(locPart.Substring(comma + 1), out int c))
                {
                    filePath = errStr.Substring(0, openParen).Trim();
                    line = l;
                    col = c;
                    cleanMsg = errStr.Substring(closeParen + 1).Trim();
                    if (cleanMsg.StartsWith(':')) cleanMsg = cleanMsg.Substring(1).Trim();
                    return true;
                }
            }
        }

        return false;
    }

    private static string? GetSuggestion(string kind, string message)
    {
        string msgLower = message.ToLowerInvariant();

        if (kind == "lexer")
        {
            if (msgLower.Contains("unexpected character"))
            {
                return "Remove this character or check if it is a typo.";
            }
            if (msgLower.Contains("unterminated string"))
            {
                return "Add a closing double quote '\"' at the end of the string literal.";
            }
        }
        else if (kind == "parser")
        {
            if (msgLower.Contains("expected 'trait' or 'class' after 'sealed'"))
            {
                return "Did you mean 'sealed trait' or 'sealed class'? Only traits and classes can be sealed.";
            }
            if (msgLower.Contains("expected 'class' or 'object' after 'case'"))
            {
                return "Did you mean 'case class' or 'case object'?";
            }
            if (msgLower.Contains("unknown annotation"))
            {
                return "SSharp only supports '@tailrec' annotation. Check for spelling errors.";
            }
            if (msgLower.Contains("annotation '@tailrec' can only be applied to functions"))
            {
                return "Remove '@tailrec' or place it directly before a function definition ('def').";
            }
            if (msgLower.Contains("expected string literal after 'import'"))
            {
                return "Ensure the import path is enclosed in double quotes, e.g., import \"SSharp.Runtime\";";
            }
            if (msgLower.Contains("expected trait name") || msgLower.Contains("expected class name") || msgLower.Contains("expected function name") || msgLower.Contains("expected value name"))
            {
                return "Provide a valid identifier name starting with a letter.";
            }
            if (msgLower.Contains("expected type parameter name"))
            {
                return "Provide a valid type identifier (e.g. A, T).";
            }
            if (msgLower.Contains("expected ']'"))
            {
                return "Add a closing bracket ']' to close the generic parameter/argument list.";
            }
            if (msgLower.Contains("expected ':' after parameter name"))
            {
                return "Add a colon ':' followed by the parameter type, e.g., (x: Int).";
            }
            if (msgLower.Contains("expected ')'"))
            {
                return "Add a closing parenthesis ')'.";
            }
            if (msgLower.Contains("expected '=' before function body"))
            {
                return "Add '=' before the function body, e.g., def myFunc() = ...";
            }
            if (msgLower.Contains("expected '=' after value name"))
            {
                return "Add '=' followed by the initial value expression, e.g., val x = 10;";
            }
            if (msgLower.Contains("expected type name"))
            {
                return "Provide a valid type identifier (e.g. Int, Double, String, Boolean, Unit, Any).";
            }
            if (msgLower.Contains("expected '=>'"))
            {
                return "Use the arrow operator '=>' here.";
            }
            if (msgLower.Contains("expected '{' after match"))
            {
                return "Add an opening brace '{' to start the match expression block.";
            }
            if (msgLower.Contains("expected '}'"))
            {
                return "Add a closing brace '}' to close the block or match expression.";
            }
            if (msgLower.Contains("expected 'case'"))
            {
                return "Write a match case starting with 'case', e.g. case Nil => 0";
            }
            if (msgLower.Contains("expected pattern"))
            {
                return "Write a valid pattern (e.g., wildcard '_', a literal, an identifier, or a constructor pattern).";
            }
            if (msgLower.Contains("expected '(' after 'if'"))
            {
                return "Enclose the 'if' condition in parentheses, e.g., if (condition) ...";
            }
            if (msgLower.Contains("expected 'else'"))
            {
                return "All 'if' expressions in SSharp must have an 'else' branch. Add 'else <expression>'.";
            }
        }
        else if (kind == "type")
        {
            if (msgLower.Contains("type mismatch: val") || msgLower.Contains("type mismatch in function"))
            {
                return "Ensure the expression type matches the declared type. You may need to change the declared type or convert the expression.";
            }
            if (msgLower.Contains("could not optimize @tailrec"))
            {
                return "Ensure the function calls itself recursively in tail position, or remove '@tailrec'.";
            }
            if (msgLower.Contains("not found in current scope"))
            {
                return "Declare the identifier, check its spelling, or check its scope and imports.";
            }
            if (msgLower.Contains("cannot be applied to types") || msgLower.Contains("cannot be applied to type"))
            {
                return "Check the types of the operands. You might need to cast or convert one of them (e.g. using .toDouble()).";
            }
            if (msgLower.Contains("if condition must be boolean"))
            {
                return "Change the condition expression to evaluate to a Boolean value.";
            }
            if (msgLower.Contains("expected") && msgLower.Contains("arguments, but got"))
            {
                return "Check the function signature and pass the correct number of arguments.";
            }
            if (msgLower.Contains("type mismatch") && msgLower.Contains("argument"))
            {
                return "Pass an argument of the correct type, or adjust the parameter type.";
            }
            if (msgLower.Contains("is not callable"))
            {
                return "Make sure you are calling a function, constructor, or valid callable object.";
            }
            if (msgLower.Contains("not in tail position"))
            {
                return "Rewrite the function so that the recursive call is the final operation (e.g., return it directly without further operations like addition/multiplication).";
            }
        }
        else if (kind == "compile")
        {
            if (msgLower.Contains("does not exist in the current context") || msgLower.Contains("could not be found"))
            {
                return "Check if a reference or using directive is missing, or check the spelling.";
            }
        }

        return null;
    }

    // Helper to display errors in a Rust-like style
    private static void PrintError(string kind, string message, string file, int? line = null, int? col = null)
    {
        int l = line ?? 1;
        int c = col ?? 1;
        string cleanMsg = message;

        if (!line.HasValue || !col.HasValue)
        {
            TryParseError(kind, message, out string? filePath, out l, out c, out cleanMsg);
            if (filePath != null && File.Exists(filePath))
            {
                file = filePath;
            }
        }

        if (cleanMsg.StartsWith("Type Error: ")) cleanMsg = cleanMsg.Substring("Type Error: ".Length);
        if (cleanMsg.StartsWith("Error at end: ")) cleanMsg = cleanMsg.Substring("Error at end: ".Length);

        Console.Error.WriteLine($"error[{kind}]: {cleanMsg}");
        Console.Error.WriteLine($"  --> {file}:{l}:{c}");

        try
        {
            if (File.Exists(file))
            {
                string[] sourceLines = File.ReadAllLines(file);
                if (l >= 1 && l <= sourceLines.Length)
                {
                    string rawLine = sourceLines[l - 1];
                    var visualLine = new System.Text.StringBuilder();
                    var caretLine = new System.Text.StringBuilder();
                    for (int i = 0; i < rawLine.Length; i++)
                    {
                        char ch = rawLine[i];
                        if (ch == '\t')
                        {
                            visualLine.Append("    ");
                            if (i < c - 1)
                            {
                                caretLine.Append("    ");
                            }
                        }
                        else
                        {
                            visualLine.Append(ch);
                            if (i < c - 1)
                            {
                                caretLine.Append(' ');
                            }
                        }
                    }

                    int caretWidth = 1;
                    int lexemeIndex = cleanMsg.IndexOf("Error at '");
                    if (lexemeIndex >= 0)
                    {
                        int endQuote = cleanMsg.IndexOf("':", lexemeIndex + 10);
                        if (endQuote > 0)
                        {
                            string lexeme = cleanMsg.Substring(lexemeIndex + 10, endQuote - (lexemeIndex + 10));
                            caretWidth = Math.Max(1, lexeme.Length);
                        }
                    }

                    if (caretWidth == 1 && c - 1 >= 0 && c - 1 < rawLine.Length)
                    {
                        char startChar = rawLine[c - 1];
                        if (char.IsLetterOrDigit(startChar) || startChar == '_')
                        {
                            int len = 0;
                            while (c - 1 + len < rawLine.Length)
                            {
                                char ch = rawLine[c - 1 + len];
                                if (char.IsLetterOrDigit(ch) || ch == '_')
                                {
                                    len++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            caretWidth = Math.Max(1, len);
                        }
                    }

                    for (int i = 0; i < caretWidth; i++)
                    {
                        caretLine.Append('^');
                    }

                    string lineNumStr = l.ToString();
                    string padding = new string(' ', lineNumStr.Length);
                    Console.Error.WriteLine($" {padding} |");
                    Console.Error.WriteLine($" {lineNumStr} | {visualLine}");
                    Console.Error.WriteLine($" {padding} | {caretLine}");
                }
            }
        }
        catch
        {
            // Ignore preview printing issues and fallback gracefully
        }

        string? suggestion = GetSuggestion(kind, cleanMsg);
        if (suggestion != null)
        {
            string lineNumStr = l.ToString();
            string padding = new string(' ', lineNumStr.Length);
            Console.Error.WriteLine($" {padding} |");
            Console.Error.WriteLine($" {padding} = help: {suggestion}");
        }
        Console.Error.WriteLine();
    }
    // Existing PrintUsage method follows
    private static void PrintUsage()
    {
        Console.WriteLine("Usage: SSharp.CLI <input-file.ss> [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -o <output-file>     Output path for the generated .cs file");
        Console.WriteLine("                       (defaults to <input-file>.cs)");
        Console.WriteLine("  -c, --compile        Compile the generated C# to a .NET executable (.dll)");
        Console.WriteLine("                       (defaults to <input-file>.dll)");
        Console.WriteLine("  -r, --run            Compile and run the SSharp program");
        Console.WriteLine("  --out-dll <path>     Output path for the compiled .dll");
        Console.WriteLine("                       (only used with -c / --compile or -r / --run)");
        Console.WriteLine("  --runtime-dll <path> Explicit path to SSharp.Runtime.dll");
        Console.WriteLine("                       (only used with -c / --compile or -r / --run)");
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
        bool   doRun        = false;

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

                case "-r":
                case "--run":
                    doRun = true;
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

        int totalSteps = doRun ? 3 : (doCompile ? 2 : 1);

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
                    PrintError("lexer", $"[{err.Line}:{err.Column}] {err.Lexeme}", inputFile, err.Line, err.Column);
                return 1;
            }

            // ── 2. Parsing ───────────────────────────────────────────────────
            var parser = new Parser(tokens);
            var ast    = parser.ParseProgram();

            if (parser.Errors.Any())
            {
                Console.Error.WriteLine("Parser errors:");
                foreach (var err in parser.Errors)
                    PrintError("parser", err, inputFile);
                return 1;
            }

            // ── 3. Type Checking ─────────────────────────────────────────────
            var typeChecker = new TypeChecker();
            typeChecker.Check(ast);

            if (typeChecker.Errors.Any())
            {
                Console.Error.WriteLine("Type errors:");
                foreach (var err in typeChecker.Errors)
                    PrintError("type", err, inputFile);
                return 1;
            }

            // ── 4. Code Generation ───────────────────────────────────────────
            var codeGen       = new CodeGenerator(typeChecker.ResolvedTypes);
            string generatedCs = codeGen.Generate(ast);

            File.WriteAllText(outputCs, generatedCs);
            Console.WriteLine($"  [1/{totalSteps}] C# source  → {outputCs}");

            // ── 5. (Optional) Roslyn Compilation ────────────────────────────
            if (!doCompile)
            {
                Console.WriteLine("Done. (Use -c to also compile to a .NET executable.)");
                return 0;
            }

            Console.WriteLine($"  [2/{totalSteps}] Compiling  → {outputDll}");

            var backend = new CSharpBackend();
            var result  = backend.Compile(
                generatedCs,
                outputDll,
                string.IsNullOrEmpty(runtimeDll) ? null : runtimeDll);

            if (!result.Success)
            {
                Console.Error.WriteLine("Roslyn compilation errors:");
                foreach (var err in result.Errors)
                    PrintError("compile", err, outputDll);
                return 1;
            }

            if (!doRun)
            {
                Console.WriteLine($"Done. Run with: dotnet {outputDll}");
                return 0;
            }

            Console.WriteLine($"  [3/3] Running    → {outputDll}");
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"\"{outputDll}\"",
                    UseShellExecute = false
                };
                using var process = System.Diagnostics.Process.Start(psi);
                if (process == null)
                {
                    Console.Error.WriteLine("Error: Failed to start the dotnet process.");
                    return 1;
                }
                process.WaitForExit();
                return process.ExitCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error during execution: {ex.Message}");
                return 1;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }
}
