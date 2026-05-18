using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Binding;
using GitHooks.Workflow.Application.Binding.Exceptions;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Tests.Binding;

public sealed class ParameterBindingContextTests
{
    [Fact]
    public void Create_WithDefaultsAndOverrides_ResolvesOverrideValue()
    {
        var span = CreateSpan();
        var parameters = new List<ParameterNode>()
        {
            CreateParameter("environment", ParameterType.Text, "dev", span),
            CreateParameter("retries", ParameterType.Number, "1", span)
        };

        var context = ParameterBindingContext.Create(
            parameters,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["environment"] = "prod"
            }
        );

        var result = context.GetResolvedValue("environment", span);

        Assert.Equal("prod", result);
    }

    [Fact]
    public void Create_WithDuplicateDeclarations_ThrowsPipelineParameterBindingException()
    {
        var span = CreateSpan();
        var parameters = new List<ParameterNode>()
        {
            CreateParameter("environment", ParameterType.Text, "dev", span),
            CreateParameter("environment", ParameterType.Text, "prod", span)
        };

        var exception = Assert.Throws<PipelineParameterBindingException>(() => ParameterBindingContext.Create(parameters));

        Assert.Contains("Parameter 'environment' is declared more than once.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Create_WithUnknownOverride_ThrowsPipelineParameterBindingException()
    {
        var span = CreateSpan();
        var parameters = new List<ParameterNode>()
        {
            CreateParameter("environment", ParameterType.Text, "dev", span)
        };

        var overrides = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["unknown"] = "value"
        };

        var exception = Assert.Throws<PipelineParameterBindingException>(() => ParameterBindingContext.Create(parameters, overrides));

        Assert.Contains("Parameter 'unknown' is not defined.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Create_WithInvalidBooleanDefault_ThrowsPipelineParameterBindingException()
    {
        var span = CreateSpan();
        var parameters = new List<ParameterNode>()
        {
            CreateParameter("enabled", ParameterType.Boolean, "yes", span)
        };

        var exception = Assert.Throws<PipelineParameterBindingException>(() => ParameterBindingContext.Create(parameters));

        Assert.Contains("Parameter 'enabled' expects a boolean value", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Create_WithInvalidNumberOverride_ThrowsPipelineParameterBindingException()
    {
        var span = CreateSpan();
        var parameters = new List<ParameterNode>()
        {
            CreateParameter("retries", ParameterType.Number, "2", span)
        };

        var overrides = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["retries"] = "two"
        };

        var exception = Assert.Throws<PipelineParameterBindingException>(() => ParameterBindingContext.Create(parameters, overrides));

        Assert.Contains("Parameter 'retries' expects a number value", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Create_WithOverrideOutsideAllowedValues_ThrowsPipelineParameterBindingException()
    {
        var span = CreateSpan();
        var parameters = new List<ParameterNode>()
        {
            CreateParameter("environment", ParameterType.Text, "dev", span, ["dev", "stage", "prod"])
        };

        var overrides = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["environment"] = "qa"
        };

        var exception = Assert.Throws<PipelineParameterBindingException>(() => ParameterBindingContext.Create(parameters, overrides));

        Assert.Contains("Parameter 'environment' value 'qa' is not in the allowed values list.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void GetResolvedValue_WithDeclaredParameterWithoutValue_ThrowsPipelineParameterBindingException()
    {
        var span = CreateSpan();
        var parameters = new List<ParameterNode>()
        {
            CreateParameter("environment", ParameterType.Text, null, span)
        };

        var context = ParameterBindingContext.Create(parameters);

        var exception = Assert.Throws<PipelineParameterBindingException>(() => context.GetResolvedValue("environment", span));

        Assert.Contains("Parameter 'environment' does not have a value.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void GetResolvedValue_WithUndefinedParameter_ThrowsPipelineParameterBindingException()
    {
        var span = CreateSpan();
        var parameters = new List<ParameterNode>()
        {
            CreateParameter("environment", ParameterType.Text, "dev", span)
        };

        var context = ParameterBindingContext.Create(parameters);

        var exception = Assert.Throws<PipelineParameterBindingException>(() => context.GetResolvedValue("region", span));

        Assert.Contains("Parameter 'region' is not defined.", exception.Message, StringComparison.Ordinal);
    }

    private static ParameterNode CreateParameter(
        string name,
        ParameterType type,
        string? defaultValue,
        SourceSpan span,
        IReadOnlyList<string>? values = null)
    {
        return new ParameterNode(name, null, type, defaultValue, values ?? [], [], span);
    }

    private static SourceSpan CreateSpan()
    {
        return SourceSpan.Unknown(new SourceRef("pipeline.yml"));
    }
}
