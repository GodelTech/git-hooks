using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Binding;
using GitHooks.Workflow.Application.Binding.Core;
using GitHooks.Workflow.Application.Binding.Exceptions;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Tests.Binding.Core;

public sealed class StringParameterBinderTests
{
    [Fact]
    public void Bind_WithEmptyValue_ReturnsInput()
    {
        var binder = new StringParameterBinder();
        var span = CreateSpan();
        var context = CreateContext(("owner", "contoso"));

        var result = binder.Bind(string.Empty, span, context);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Bind_WithoutInterpolation_ReturnsInput()
    {
        var binder = new StringParameterBinder();
        var span = CreateSpan();
        var context = CreateContext(("owner", "contoso"));

        var result = binder.Bind("plain text", span, context);

        Assert.Equal("plain text", result);
    }

    [Fact]
    public void Bind_WithMultipleParameterExpressions_ReplacesEachExpression()
    {
        var binder = new StringParameterBinder();
        var span = CreateSpan();
        var context = CreateContext(("owner", "contoso"), ("repository", "git-hooks"));

        var result = binder.Bind("https://example.com/${{ parameters.owner }}/${{ parameters.repository }}", span, context);

        Assert.Equal("https://example.com/contoso/git-hooks", result);
    }

    [Fact]
    public void Bind_WithUnsupportedExpression_ThrowsPipelineParameterBindingException()
    {
        var binder = new StringParameterBinder();
        var span = CreateSpan();
        var context = CreateContext(("owner", "contoso"));

        var exception = Assert.Throws<PipelineParameterBindingException>(
            () => binder.Bind("${{ variables.owner }}", span, context)
        );

        Assert.Contains("Unsupported expression 'variables.owner'", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Bind_WithMissingParameterName_ThrowsPipelineParameterBindingException()
    {
        var binder = new StringParameterBinder();
        var span = CreateSpan();
        var context = CreateContext(("owner", "contoso"));

        var exception = Assert.Throws<PipelineParameterBindingException>(
            () => binder.Bind("${{ parameters. }}", span, context)
        );

        Assert.Contains("Parameter expression must specify a parameter name.", exception.Message, StringComparison.Ordinal);
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
