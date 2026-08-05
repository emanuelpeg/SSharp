# SSharp VS Code Extension

Official Visual Studio Code extension for **SSharp** — a statically-typed, expression-based functional programming language that transpiles to C#.

## Features

- **Syntax Highlighting**: Full color support for keywords, control flow (`if`/`else`/`match`/`case`), annotations (`@tailrec`), primitives (`Int`, `Double`, `String`, `Boolean`, `Unit`, `Any`), ADTs (`List`, `Option`, `Set`, `Map`, `Tuple2`, `Some`, `None`, `Nil`, `Cons`), operators (`::`, `=>`, etc.), literals, strings, and comments.
- **Language Configuration**: Bracket matching, auto-closing quotes/braces, and block comment toggling (`//` and `/* ... */`).

## File Extensions Supported

- `.ss`
- `.ssharp`

## Installation for Local Development

### Option 1: Direct Copy / Symlink (Recommended for quick testing)

Copy the `vscode-ssharp` folder to your VS Code extensions folder:

**Windows**:
```powershell
xcopy /E /I "c:\Users\emanuel.goette\dotnet\SSharp\vscode-ssharp" "$env:USERPROFILE\.vscode\extensions\vscode-ssharp"
```

Restart VS Code or run **Developer: Reload Window** (`Ctrl+Shift+P` -> `Reload Window`).

### Option 2: Install via VSIX

If you have `vsce` installed:

```bash
npm install -g @vscode/vsce
cd vscode-ssharp
vsce package
code --install-extension vscode-ssharp-0.1.0.vsix
```

## License

MIT
