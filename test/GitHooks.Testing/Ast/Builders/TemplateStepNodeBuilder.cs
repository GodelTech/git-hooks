using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Common;

namespace GitHooks.Testing.Ast.Builders;

public sealed class TemplateStepNodeBuilder
    : MappingNodeBuilder<TemplateStepNodeBuilder>
{
    private readonly List<StringKeyFieldNode<ExpressionNode>> _parameters = [];

    private StringKeyFieldNode<ExpressionNode>? _template;

    public TemplateStepNodeBuilder()
    {
        WithTemplate("build.yml");
    }

    public TemplateStepNodeBuilder WithTemplate(
        string template = "build.yml")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);

        _template = TestFields.StringKey(
            "template",
            template);

        return this;
    }

    public TemplateStepNodeBuilder WithParameter(
        string name,
        ExpressionNode value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(value);

        _parameters.Add(
            new StringKeyFieldNode<ExpressionNode>
            {
                Key = name,
                Value = value,
                Span = SourceSpan.Unknown
            });

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

    public TemplateStepNode Build()
    {
        if (_template is null)
        {
            throw new InvalidOperationException("Template is required to build a TemplateStepNode.");
        }

        return new TemplateStepNode
        {
            Template = _template,
            Parameters = _parameters.Count == 0
                ? null
                : new MappingFieldNode<StringKeyFieldNode<ExpressionNode>>
                {
                    Key = "parameters",
                    Fields = [.. _parameters],
                    Span = SourceSpan.Unknown
                },
            UnknownFields = [.. UnknownFields],
            Span = Span
        };
    }
}
