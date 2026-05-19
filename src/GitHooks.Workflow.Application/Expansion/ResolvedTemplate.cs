using GitHooks.Workflow.Application.Ast;

namespace GitHooks.Workflow.Application.Expansion;

/// <summary>
/// Represents a fully resolved template pipeline.
/// </summary>
public sealed record ResolvedTemplate(
    string TemplatePath,
    PipelineNode Pipeline);
