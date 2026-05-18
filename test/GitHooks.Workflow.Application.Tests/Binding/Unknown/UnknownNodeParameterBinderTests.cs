using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Application.Binding;
using GitHooks.Workflow.Application.Binding.Core;
using GitHooks.Workflow.Application.Binding.Exceptions;
using GitHooks.Workflow.Application.Binding.Unknown;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Tests.Binding.Unknown;

public sealed class UnknownNodeParameterBinderTests
{
    [Fact]
    public void Bind_WithUnsupportedExpressionInUnknownField_ThrowsPipelineParameterBindingException()
    {
        var binder = CreateBinder();
        var span = CreateSpan();
        var context = CreateContext(("owner", "contoso"));

        var unknownFields = new List<UnknownFieldNode>()
        {
            new(
                new UnknownScalarNode("x", span),
                new UnknownScalarNode("${{ variables.owner }}", span),
                span
            )
        };

        var exception = Assert.Throws<PipelineParameterBindingException>(() => binder.BindUnknownFields(unknownFields, context));

        Assert.Contains("Unsupported expression 'variables.owner'", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Bind_WithNestedUnknownNodes_ReplacesInterpolatedScalarValues()
    {
        var binder = CreateBinder();
        var span = CreateSpan();
        var context = CreateContext(
            ("owner", "default-owner"),
            ("repo", "default-repo")
        );

        var unknownFields = new List<UnknownFieldNode>()
        {
            new(
                new UnknownScalarNode("meta", span),
                new UnknownMappingNode(
                [
                    new UnknownMappingEntryNode(
                        new UnknownScalarNode("owner", span),
                        new UnknownScalarNode("${{ parameters.owner }}", span),
                        span
                    ),
                    new UnknownMappingEntryNode(
                        new UnknownScalarNode("items", span),
                        new UnknownSequenceNode(
                        [
                            new UnknownScalarNode("${{ parameters.repo }}", span),
                            new UnknownReferenceNode("preserve-me", span)
                        ],
                        span
                        ),
                        span
                    )
                ],
                span
                ),
                span
            )
        };

        var result = binder.BindUnknownFields(unknownFields, context);

        var stepUnknownField = Assert.Single(result);
        var metaKey = Assert.IsType<UnknownScalarNode>(stepUnknownField.Key);
        Assert.Equal("meta", metaKey.Value);

        var metaValue = Assert.IsType<UnknownMappingNode>(stepUnknownField.Value);

        var ownerEntry = metaValue.Entries[0];
        var ownerValue = Assert.IsType<UnknownScalarNode>(ownerEntry.Value);
        Assert.Equal("default-owner", ownerValue.Value);

        var itemsEntry = metaValue.Entries[1];
        var itemsValue = Assert.IsType<UnknownSequenceNode>(itemsEntry.Value);

        var firstItem = Assert.IsType<UnknownScalarNode>(itemsValue.Items[0]);
        Assert.Equal("default-repo", firstItem.Value);

        var secondItem = Assert.IsType<UnknownReferenceNode>(itemsValue.Items[1]);
        Assert.Equal("preserve-me", secondItem.Value);
    }

    [Fact]
    public void Bind_WithInterpolatedUnknownKey_ReplacesKeyValue()
    {
        var binder = CreateBinder();
        var span = CreateSpan();
        var context = CreateContext(("fieldName", "metadata"));

        var unknownFields = new List<UnknownFieldNode>()
        {
            new(
                new UnknownScalarNode("${{ parameters.fieldName }}", span),
                new UnknownScalarNode("literal", span),
                span
            )
        };

        var result = binder.BindUnknownFields(unknownFields, context);

        var unknownField = Assert.Single(result);
        var key = Assert.IsType<UnknownScalarNode>(unknownField.Key);
        Assert.Equal("metadata", key.Value);

        var value = Assert.IsType<UnknownScalarNode>(unknownField.Value);
        Assert.Equal("literal", value.Value);
    }

    [Fact]
    public void Bind_WithUnsupportedUnknownNodeType_ThrowsPipelineParameterBindingException()
    {
        var binder = CreateBinder();
        var span = CreateSpan();
        var context = CreateContext(("owner", "contoso"));

        var unknownFields = new List<UnknownFieldNode>()
        {
            new(
                new UnsupportedUnknownNode(span),
                new UnknownScalarNode("value", span),
                span
            )
        };

        var exception = Assert.Throws<PipelineParameterBindingException>(() => binder.BindUnknownFields(unknownFields, context));

        Assert.Contains("Unsupported unknown node type", exception.Message, StringComparison.Ordinal);
        Assert.Contains(nameof(UnsupportedUnknownNode), exception.Message, StringComparison.Ordinal);
    }

    private static UnknownNodeParameterBinder CreateBinder()
    {
        return new UnknownNodeParameterBinder(new StringParameterBinder());
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

    private sealed record UnsupportedUnknownNode(SourceSpan Span)
        : UnknownNode(Span);
}
