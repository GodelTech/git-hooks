using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Ast;

public sealed record TemplateStepNode(
    string Template,
    IReadOnlyDictionary<string, InterpolatedStringNode> Parameters,
    IReadOnlyList<UnknownFieldNode> UnknownFields,
    SourceSpan Span)
    : StepNode(UnknownFields, Span);
