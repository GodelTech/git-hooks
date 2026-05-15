using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Application.Binding.Parameters;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Binding;

internal static class ParameterBindingHelper
{
    public static Dictionary<string, InterpolatedStringNode> BindInterpolatedStringMap(
        IReadOnlyDictionary<string, InterpolatedStringNode> values,
        ParameterBindingContext context)
    {
        return values.ToDictionary(
            pair => pair.Key,
            pair => new InterpolatedStringNode(
                ParameterScalarBinder.Bind(pair.Value.Value, SourceSpan.Unknown(new SourceRef(pair.Key)), context)
            ),
            StringComparer.Ordinal
        );
    }
}
