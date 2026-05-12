using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Ast;

public abstract record StepNode(IReadOnlyList<UnknownFieldNode> UnknownFields, SourceSpan Span)
    : AstNode(UnknownFields, Span);
