using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Application.Binding;
using GitHooks.Workflow.Application.Binding.Core;
using GitHooks.Workflow.Application.Binding.Exceptions;
using GitHooks.Workflow.Application.Binding.Expressions;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Tests.Binding.Expressions;

public sealed class InterpolationParameterBinderTests
{
    [Fact]
    public void Bind_WithoutInterpolation_ReturnsSameValue()
    {
        var binder = CreateBinder();
        var span = CreateSpan();
        var context = CreateContext(("owner", "contoso"));

        var result = binder.Bind(new InterpolatedStringNode("plain text"), span, context);

        Assert.Equal("plain text", result.Value);
    }

    [Fact]
    public void Bind_WithParameterExpression_ReplacesExpression()
    {
        var binder = CreateBinder();
        var span = CreateSpan();
        var context = CreateContext(("owner", "contoso"));

        var result = binder.Bind(new InterpolatedStringNode("repo/${{ parameters.owner }}"), span, context);

        Assert.Equal("repo/contoso", result.Value);
    }

    [Fact]
    public void Bind_WithUnsupportedExpression_ThrowsPipelineParameterBindingException()
    {
        var binder = CreateBinder();
        var span = CreateSpan();
        var context = CreateContext(("owner", "contoso"));

        var exception = Assert.Throws<PipelineParameterBindingException>(
            () => binder.Bind(new InterpolatedStringNode("${{ variables.owner }}"), span, context)
        );

        Assert.Contains("Unsupported expression 'variables.owner'", exception.Message, StringComparison.Ordinal);
    }

    private static InterpolationParameterBinder CreateBinder()
    {
        return new InterpolationParameterBinder(new StringParameterBinder());
    }

    private static ParameterBindingContext CreateContext(params (string Name, string? DefaultValue)[] parameters)
    {
        var span = CreateSpan();

        var declarations = parameters
            .Select(parameter => new ParameterNode(parameter.Name, null, ParameterType.Text, parameter.DefaultValue, [], [], span))
            .ToList();

        return ParameterBindingContext.Create(declarations);
    }

    private static SourceSpan CreateSpan()
    {
        return SourceSpan.Unknown(new SourceRef("pipeline.yml"));
    }
}
