using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Common;
using GitHooks.Testing.Ast.Builders.Fields;

namespace GitHooks.Testing.Ast.Builders.Mappings.Steps;

public sealed class TemplateStepNodeBuilder
    : StepNodeBuilder<TemplateStepNodeBuilder, TemplateStepNode>
{
    private readonly List<StringKeyFieldNode<ExpressionNode>> _parameters = [];

    private StringKeyFieldNode<ExpressionNode>? _template;

    public TemplateStepNodeBuilder WithTemplate(
        Action<StringKeyFieldNodeBuilder> configure)
    {
        _template = Configure(configure).Build();

        return Self;
    }

    public TemplateStepNodeBuilder WithTemplate(
        string template)
    {
        return WithTemplate(x => x
            .WithKey("template")
            .WithStringValue(v => v.WithValue(template)));
    }

    public TemplateStepNodeBuilder WithParameter(
        Action<StringKeyFieldNodeBuilder> configure)
    {
        return WithParameter(
            Configure(configure).Build());
    }

    public TemplateStepNodeBuilder WithParameter(
        string key,
        string value)
    {
        return WithParameter(x => x
            .WithKey(key)
            .WithStringValue(v => v.WithValue(value)));
    }

    public override TemplateStepNode Build()
    {
        if (_template is null)
        {
            throw new InvalidOperationException(
                "Template is required to build a TemplateStepNode.");
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

    private TemplateStepNodeBuilder WithParameter(
        StringKeyFieldNode<ExpressionNode> parameter)
    {
        _parameters.Add(parameter);

        return Self;
    }
}
