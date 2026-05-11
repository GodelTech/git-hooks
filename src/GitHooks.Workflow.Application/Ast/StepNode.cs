using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Ast;

public abstract record StepNode(IReadOnlyList<UnknownFieldNode> UnknownFields, SourceSpan Span)
    : AstNode(UnknownFields, Span)
{
    public string? DisplayName { get; init; }
    public ExpressionNode? Condition { get; init; }
    public int? TimeoutInMinutes { get; init; } // todo: consider using TimeSpan instead of int for better clarity and flexibility

    public InterpolatedStringNode? WorkingDirectory { get; init; }

    public IReadOnlyDictionary<string, InterpolatedStringNode> Env { get; init; }
        = new Dictionary<string, InterpolatedStringNode>();
}

