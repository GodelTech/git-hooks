using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Ast;

public sealed record ScriptStepNode(InterpolatedStringNode Script, SourceSpan Span)
    : StepNode(Span);
