namespace GitHooks.Pipeline.Domain.Model;

/// <summary>
/// Represents a 1-based position in the source document.
/// </summary>
/// <param name="Line">The 1-based line number.</param>
/// <param name="Column">The 1-based column number.</param>
public readonly record struct SourceLocation(long Line, long Column);
