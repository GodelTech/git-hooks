using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Ast;

public abstract record AstNode(SourceSpan Span);
