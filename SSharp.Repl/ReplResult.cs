using System.Collections.Generic;

namespace SSharp.Repl;

/// <summary>Result returned by <see cref="ReplSession.Submit"/>.</summary>
public record ReplResult(
    bool IsSuccess,
    /// <summary>Captured stdout from executing the snippet.</summary>
    string Output,
    IReadOnlyList<string> Errors,
    long ElapsedMs,
    /// <summary>
    /// Human-readable SSharp type of the last top-level binding or expression,
    /// e.g. "Int", "List[String]". Null when the snippet defines no evaluatable expression.
    /// </summary>
    string? TypeInfo = null,
    /// <summary>
    /// The name of the last binding introduced, if any (e.g. "x" for <c>val x = 42</c>).
    /// </summary>
    string? BindingName = null);
