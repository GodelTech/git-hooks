using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Expansion;

/// <summary>
/// Represents a fully resolved template pipeline.
/// </summary>
public sealed record ResolvedTemplate(
    PipelineSource TemplateSource,
    PipelineNode Pipeline);
