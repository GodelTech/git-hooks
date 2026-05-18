using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Application.Binding.Core;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Binding.Expressions;

internal sealed class InterpolationParameterBinder(StringParameterBinder stringParameterBinder)
{
    private readonly StringParameterBinder _stringParameterBinder = stringParameterBinder;

    public InterpolatedStringNode Bind(InterpolatedStringNode node, SourceSpan span, ParameterBindingContext context)
    {
        return new InterpolatedStringNode(
            _stringParameterBinder.Bind(node.Value, span, context)
        );
    }
}
