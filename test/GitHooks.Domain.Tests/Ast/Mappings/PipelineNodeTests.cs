using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Testing.Ast;
using GitHooks.Testing.Common;

namespace GitHooks.Domain.Tests.Ast.Mappings;

public sealed class PipelineNodeTests
{
    [Fact]
    public void Empty_CreatesEmptyPipeline()
    {
        var document = TestSourceDocument.Default;

        var span = TestSourceSpan.Create(document, 1, 1, 1, 1);

        var pipeline = PipelineNode.Empty(span);

        PipelineAssert.IsEmptyPipeline(
            pipeline,
            span);
    }

    [Fact]
    public void Empty_ReturnsNewInstance()
    {
        var first = PipelineNode.Empty(TestSourceSpan.Unknown);
        var second = PipelineNode.Empty(TestSourceSpan.Unknown);

        Assert.NotSame(
            first,
            second);
    }

    [Fact]
    public void Empty_SetsKind()
    {
        var pipeline = PipelineNode.Empty(TestSourceSpan.Unknown);

        Assert.Equal(
            AstNodeKind.Pipeline,
            pipeline.Kind);
    }
}
