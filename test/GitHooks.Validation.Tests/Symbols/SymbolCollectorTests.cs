using GitHooks.Testing.Ast.Builders.Mappings;
using GitHooks.Validation.Extensions;
using GitHooks.Validation.Symbols;

namespace GitHooks.Validation.Tests.Symbols;

public sealed class SymbolCollectorTests
{
    [Fact]
    public void Constructor_NullContext_Throws()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => new SymbolCollector(null!));

        Assert.Equal(
            "context",
            exception.ParamName);
    }

    [Fact]
    public void Collect_NullRoot_Throws()
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var collector = new SymbolCollector(context);

        var exception =
            Assert.Throws<ArgumentNullException>(
                () => collector.Collect(null!));

        Assert.Equal(
            "root",
            exception.ParamName);
    }

    [Fact]
    public void Collect_PipelineWithNoParameters_DoesNotCollectSymbols()
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var collector = new SymbolCollector(context);

        var pipeline = new PipelineNodeBuilder()
            .WithoutParameters()
            .WithScriptStep(x => x
                .WithScript("dotnet test"))
            .Build();

        collector.Collect(pipeline);

        Assert.False(
            context.Symbols.TryGetParameter(
                "configuration",
                out _));
    }

    [Fact]
    public void Collect_PipelineWithParameter_CollectsParameter()
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var collector = new SymbolCollector(context);

        var pipeline = new PipelineNodeBuilder()
            .WithParameter(x => x
                .WithName("configuration"))
            .Build();

        collector.Collect(pipeline);

        Assert.True(
            context.Symbols.TryGetParameter(
                "configuration",
                out var parameter));

        Assert.Equal(
            "configuration",
            parameter.GetNameForDiagnostic());
    }

    [Fact]
    public void Collect_PipelineWithMultipleParameters_CollectsAllParameters()
    {
        var context = TestValidationContextFactory.Create(out var diagnostics);

        var collector = new SymbolCollector(context);

        var pipeline = new PipelineNodeBuilder()
            .WithParameter(x => x
                .WithName("configuration"))
            .WithParameter(x => x
                .WithName("runtime"))
            .WithParameter(x => x
                .WithName("target"))
            .Build();

        var expected = pipeline.Parameters
            .ToDictionary(
                static parameter => parameter.GetNameForDiagnostic());

        collector.Collect(pipeline);

        foreach (var (name, parameter) in expected)
        {
            Assert.True(
                context.Symbols.TryGetParameter(
                    name,
                    out var actual));

            Assert.Same(
                parameter,
                actual);
        }
    }
}
