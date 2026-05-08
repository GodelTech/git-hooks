using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Ast;

public sealed record PipelineNode(IReadOnlyList<StepNode> Steps, SourceSpan Span)
    : AstNode(Span)
{
    public IReadOnlyList<UnknownFieldNode> UnknownFields { get; init; }
        = [];
}
