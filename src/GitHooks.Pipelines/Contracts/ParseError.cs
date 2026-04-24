namespace GitHooks.Pipelines.Contracts;

/// <summary>
/// Represents a single parser or validation error with location information.
/// </summary>
/// <param name="Message">The error message.</param>
/// <param name="Line">The 1-based line number, or 0 when not applicable.</param>
/// <param name="Column">The 1-based column number, or 0 when not applicable.</param>
public sealed record ParseError(string Message, long Line, long Column);
