<p align="center">
  <img src="../assets/ssharp.jpeg" alt="SSharp Logo" width="130" />
</p>

# SSharp.CLI & Interactive REPL

Command-line compiler driver and interactive REPL for the **SSharp** functional programming language.

---

## 🛠️ CLI Compiler Usage

The `SSharp.CLI` tool can transpile SSharp source files (`.ss`) to C#, compile them directly to .NET assemblies (`.dll`), and run them immediately.

```sh
# Transpile to C# source file (.cs)
dotnet run --project SSharp.CLI -- program.ss

# Transpile and compile to .NET executable assembly (.dll)
dotnet run --project SSharp.CLI -- program.ss -c

# Transpile, compile, and run immediately
dotnet run --project SSharp.CLI -- program.ss -r

# Specify custom output paths
dotnet run --project SSharp.CLI -- program.ss -c --out-dll ./build/MyProgram.dll
```

### Compiler Flags

| Flag | Long Flag | Description |
|------|-----------|-------------|
| `-o <file>` | `--out <file>` | Specify output C# file path |
| `-c` | `--compile` | Transpile and compile to a runnable .NET assembly (.dll) |
| `-r` | `--run` | Transpile, compile, and immediately execute |
| | `--out-dll <file>` | Custom destination path for compiled assembly |
| | `--runtime-dll <file>` | Custom path to `SSharp.Runtime.dll` |

---

## 💻 Interactive REPL

To start the stateful interactive session:

```sh
dotnet run --project SSharp.CLI -- repl
```

### REPL Commands

| Command | Alias | Description |
|---------|-------|-------------|
| `:help` | `:h` | Show help message and command guide |
| `:quit` | `:q` | Exit the REPL session |
| `:reset` | | Clear all accumulated definitions from the session |
| `:ctx` | `:context` | Inspect accumulated source code definitions |

### Multi-line Mode

Open a brace `{` to enter multi-line mode. SSharp will accumulate input lines until all open braces are closed before evaluating.
