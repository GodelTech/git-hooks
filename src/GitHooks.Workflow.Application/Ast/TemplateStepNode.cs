using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Ast;

public sealed record TemplateStepNode(
    string Template,
    IReadOnlyDictionary<string, InterpolatedStringNode> Parameters,
    SourceSpan Span
) : StepNode(Span);
