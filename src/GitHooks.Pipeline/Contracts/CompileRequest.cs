using GitHooks.Pipeline.Domain;

namespace GitHooks.Pipeline.Contracts;

/// <summary>
/// Represents input data required to compile a parsed pipeline into an executable plan.
/// </summary>
/// <param name="Pipeline">The parsed pipeline definition.</param>
/// <param name="QueueParameters">Queue-time parameter values keyed by parameter name.</param>
/// <param name="SourcePath">The optional source file path used for diagnostics.</param>
public sealed record CompileRequest(
    PipelineDefinition Pipeline,
    IReadOnlyDictionary<string, string?> QueueParameters,
    string? SourcePath);
