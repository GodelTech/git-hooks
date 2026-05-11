using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Ast;

public abstract record AstNode(IReadOnlyList<UnknownFieldNode> UnknownFields, SourceSpan Span);

