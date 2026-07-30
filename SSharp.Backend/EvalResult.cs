using System.Collections.Generic;

namespace SSharp.Backend;

/// <summary>Result of an in-memory evaluation of SSharp-generated C# code.</summary>
public record EvalResult(
    bool Success,
    string Output,
    IReadOnlyList<string> Errors,
    long ElapsedMs,
    /// <summary>
    /// The SSharp type of the last top-level expression or val declaration,
    /// as a human-readable string (e.g. "Int", "List[String]"). Null when not applicable.
    /// </summary>
    string? TypeInfo = null,
    /// <summary>
    /// Evaluated string representation of the value (e.g. "42", "List(1, 2, 3)").
    /// </summary>
    string? ValueString = null);
