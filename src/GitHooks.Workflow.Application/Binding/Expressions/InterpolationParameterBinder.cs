using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Binding.Expressions;

internal sealed class InterpolationParameterBinder(StringBinder stringBinder)
{
    private readonly StringBinder _stringBinder = stringBinder;

    public InterpolatedStringNode Bind(InterpolatedStringNode node, SourceSpan span, ParameterBindingContext context)
    {
        return new InterpolatedStringNode(
            _stringBinder.Bind(node.Value, span, context)
        );
    }
}
