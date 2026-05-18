using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Application.Binding;
using GitHooks.Workflow.Application.Binding.Exceptions;
using GitHooks.Workflow.Application.Binding.Unknown;
using GitHooks.Workflow.Application.DependencyInjection;
using GitHooks.Workflow.Domain.Model;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Workflow.Application.Tests.Binding.Unknown;

public sealed class UnknownNodeParameterBinderTests
{
    [Fact]
    public void Bind_WithUnsupportedExpressionInUnknownField_ThrowsPipelineParameterBindingException()
    {
        var binder = CreateBinder();
        var span = SourceSpan.Unknown(new SourceRef("pipeline.yml"));
        var parameters = new List<ParameterNode>()
        {
            new("owner", null, ParameterType.Text, "contoso", [], [], span)
        };

        var context = ParameterBindingContext.Create(parameters);

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
        var span = SourceSpan.Unknown(new SourceRef("pipeline.yml"));
        var parameters = new List<ParameterNode>()
        {
            new("owner", null, ParameterType.Text, "default-owner", [], [], span),
            new("repo", null, ParameterType.Text, "default-repo", [], [], span)
        };

        var context = ParameterBindingContext.Create(parameters);

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

    private static UnknownNodeParameterBinder CreateBinder()
    {
        var services = new ServiceCollection();
        services.AddPipelineParameterBinding();

        var provider = services.BuildServiceProvider();

        return provider.GetRequiredService<UnknownNodeParameterBinder>();
    }
}
