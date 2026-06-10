using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Common;

namespace GitHooks.Testing.Ast.Builders;

public sealed class TemplateStepNodeBuilder
    : MappingNodeBuilder<TemplateStepNodeBuilder>
{
    private readonly Dictionary<string, ExpressionNode> _parameters
        = new(StringComparer.OrdinalIgnoreCase);

    private ExpressionNode? _template;

    public TemplateStepNodeBuilder()
    {
        _template = new StringLiteralExpressionNode
        {
            Value = "build.yml",
            Span = SourceSpan.Unknown
        };
    }

    public TemplateStepNodeBuilder WithTemplate(
        ExpressionNode template)
    {
        ArgumentNullException.ThrowIfNull(template);

        _template = template;

        return this;
    }

    public TemplateStepNodeBuilder WithTemplate(
        string template = "build.yml")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);

        ArgumentNullException.ThrowIfNull(template);

        WithTemplate(
            TestExpressions.String(template));

        return this;
    }

    public TemplateStepNodeBuilder WithParameter(
        string name,
        ExpressionNode value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(value);

        _parameters.Add(name, value);

        return this;
    }

    public TemplateStepNodeBuilder WithParameter(
        string name,
        string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        WithParameter(
            name,
            TestExpressions.String(value));

        return this;
    }

    public TemplateStepNodeBuilder WithParameters(
        IReadOnlyDictionary<string, ExpressionNode> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        foreach (var pair in parameters)
        {
            WithParameter(
                pair.Key,
                pair.Value);
        }

        return this;
    }

    public TemplateStepNode Build()
    {
        if (_template is null)
        {
            throw new InvalidOperationException("Template is required to build a TemplateStepNode.");
        }

        return new TemplateStepNode
        {
            Template = _template,
            Parameters = new Dictionary<string, ExpressionNode>(_parameters),
            UnknownFields = [.. UnknownFields],
            Span = Span
        };
    }
}
