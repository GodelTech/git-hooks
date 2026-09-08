using GitHooks.Compilation.Binding;
using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Common;
using GitHooks.Testing.Ast.Builders.Mappings;
using GitHooks.Testing.Ast.Builders.Mappings.Parameters;
using GitHooks.Testing.Diagnostics;

namespace GitHooks.Compilation.Tests.Binding;

public sealed class ParameterBinderTests
{
    private readonly ParameterBinder _binder = new();

    [Fact]
    public void Prepare_NullRoot_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => _binder.Prepare(
                null!,
                null,
                new CompilationContext()));
    }

    [Fact]
    public void Prepare_NullContext_Throws()
    {
        var pipeline = PipelineNode.Empty(SourceSpan.Unknown);

        Assert.Throws<ArgumentNullException>(
            () => _binder.Prepare(
                pipeline,
                null,
                null!));
    }

    [Fact]
    public void Prepare_CollectsParametersAndSeedsDefaults()
    {
        var pipeline = new PipelineNodeBuilder()
            .WithParameter(x => x
                .WithName("serverName")
                .WithDefaultValue("localhost"))
            .WithParameter(x => x
                .WithName("port")
                .WithDefaultValue("8080"))
            .WithParameter(x => x
                .WithName("environment"))
            .Build();

        var context = new CompilationContext();

        _binder.Prepare(
            pipeline,
            null,
            context);

        Assert.Equal(3, context.Parameters.Parameters.Count);
        Assert.True(context.Parameters.ContainsParameter("serverName"));
        Assert.True(context.Parameters.ContainsParameter("port"));
        Assert.True(context.Parameters.ContainsParameter("environment"));

        Assert.True(context.Values.TryGetValue("serverName", out var serverName));
        Assert.Equal("localhost", serverName);

        Assert.True(context.Values.TryGetValue("port", out var port));
        Assert.Equal("8080", port);

        Assert.False(context.Values.ContainsValue("environment"));
        DiagnosticAssert.Empty(context.Diagnostics);
    }

    [Fact]
    public void Prepare_BooleanAndIntegerDefaults_AreSeededIntoValues()
    {
        var pipeline = new PipelineNodeBuilder()
            .WithParameter(x => x
                .WithName("enableDebug")
                .WithType("boolean")
                .WithDefaultValue(d => d
                    .WithKey("defaultValue")
                    .WithBooleanValue(v => v.WithValue(true))))
            .WithParameter(x => x
                .WithName("retries")
                .WithType("number")
                .WithDefaultValue(d => d
                    .WithKey("defaultValue")
                    .WithIntegerValue(v => v.WithValue(42))))
            .Build();

        var context = new CompilationContext();

        _binder.Prepare(
            pipeline,
            null,
            context);

        Assert.True(context.Values.TryGetValue("enableDebug", out var enableDebug));
        Assert.Equal(bool.TrueString, enableDebug);

        Assert.True(context.Values.TryGetValue("retries", out var retries));
        Assert.Equal("42", retries);

        DiagnosticAssert.Empty(context.Diagnostics);
    }

    [Fact]
    public void Prepare_SkipsBlankNamesAndKeepsFirstDuplicateDeclaration()
    {
        var first = new ParameterNodeBuilder()
            .WithName("serverName")
            .WithDefaultValue("localhost")
            .Build();

        var duplicate = new ParameterNodeBuilder()
            .WithName("serverName")
            .WithDefaultValue("remotehost")
            .Build();

        var blank = new ParameterNodeBuilder()
            .WithName("   ")
            .Build();

        var pipeline = new PipelineNodeBuilder()
            .WithParameter(first)
            .WithParameter(duplicate)
            .WithParameter(blank)
            .Build();

        var context = new CompilationContext();

        _binder.Prepare(
            pipeline,
            null,
            context);

        Assert.Single(context.Parameters.Parameters);
        Assert.True(context.Parameters.TryGetParameter("serverName", out var found));
        Assert.Same(first, found);
        Assert.True(context.Values.TryGetValue("serverName", out var value));
        Assert.Equal("localhost", value);
    }

    [Fact]
    public void Prepare_NullOverrides_IsNoOp()
    {
        var pipeline = new PipelineNodeBuilder()
            .WithParameter(x => x.WithName("serverName"))
            .Build();

        var context = new CompilationContext();

        _binder.Prepare(
            pipeline,
            null,
            context);

        Assert.False(context.Values.ContainsValue("serverName"));
        DiagnosticAssert.Empty(context.Diagnostics);
    }

    [Fact]
    public void Prepare_EmptyOverrides_IsNoOp()
    {
        var pipeline = new PipelineNodeBuilder()
            .WithParameter(x => x.WithName("serverName"))
            .Build();

        var context = new CompilationContext();

        _binder.Prepare(
            pipeline,
            new Dictionary<string, string>(),
            context);

        Assert.False(context.Values.ContainsValue("serverName"));
        DiagnosticAssert.Empty(context.Diagnostics);
    }

    [Fact]
    public void Prepare_OverrideForDeclaredParameter_SetsValueAndOverwritesDefault()
    {
        var pipeline = new PipelineNodeBuilder()
            .WithParameter(x => x
                .WithName("serverName")
                .WithDefaultValue("default-host"))
            .Build();

        var context = new CompilationContext();

        _binder.Prepare(
            pipeline,
            new Dictionary<string, string> { ["serverName"] = "localhost" },
            context);

        Assert.True(context.Values.TryGetValue("serverName", out var value));
        Assert.Equal("localhost", value);
        DiagnosticAssert.Empty(context.Diagnostics);
    }

    [Fact]
    public void Prepare_OverrideForUndeclaredParameter_ReportsDiagnosticAndDoesNotSetValue()
    {
        var pipeline = PipelineNode.Empty(SourceSpan.Unknown);
        var context = new CompilationContext();

        _binder.Prepare(
            pipeline,
            new Dictionary<string, string> { ["unknown"] = "value" },
            context);

        var diagnostic = Assert.Single(context.Diagnostics.Diagnostics);

        Assert.Equal(DiagnosticDescriptors.ParameterOverrideNotDeclared, diagnostic.Descriptor);
        Assert.Equal(SourceSpan.Unknown, diagnostic.Span);
        Assert.Equal(["unknown"], diagnostic.Arguments);
        Assert.False(context.Values.ContainsValue("unknown"));
    }

    [Fact]
    public void Prepare_OverrideNameIsCaseInsensitive_MatchesDeclaration()
    {
        var pipeline = new PipelineNodeBuilder()
            .WithParameter(x => x.WithName("serverName"))
            .Build();

        var context = new CompilationContext();

        _binder.Prepare(
            pipeline,
            new Dictionary<string, string> { ["SERVERNAME"] = "localhost" },
            context);

        Assert.True(context.Values.ContainsValue("serverName"));
        DiagnosticAssert.Empty(context.Diagnostics);
    }

    [Fact]
    public void Prepare_InvalidBooleanOverride_ReportsTypeMismatchAndDoesNotSetValue()
    {
        var pipeline = new PipelineNodeBuilder()
            .WithParameter(x => x
                .WithName("enableDebug")
                .WithType("boolean"))
            .Build();

        var context = new CompilationContext();

        _binder.Prepare(
            pipeline,
            new Dictionary<string, string> { ["enableDebug"] = "not-a-bool" },
            context);

        var diagnostic = Assert.Single(context.Diagnostics.Diagnostics);

        Assert.Equal(DiagnosticDescriptors.ParameterOverrideValueTypeMismatch, diagnostic.Descriptor);
        Assert.Equal(["enableDebug", "boolean"], diagnostic.Arguments);
        Assert.False(context.Values.ContainsValue("enableDebug"));
    }

    [Fact]
    public void Prepare_InvalidNumberOverride_ReportsTypeMismatchAndDoesNotSetValue()
    {
        var pipeline = new PipelineNodeBuilder()
            .WithParameter(x => x
                .WithName("retries")
                .WithType("number"))
            .Build();

        var context = new CompilationContext();

        _binder.Prepare(
            pipeline,
            new Dictionary<string, string> { ["retries"] = "not-a-number" },
            context);

        var diagnostic = Assert.Single(context.Diagnostics.Diagnostics);

        Assert.Equal(DiagnosticDescriptors.ParameterOverrideValueTypeMismatch, diagnostic.Descriptor);
        Assert.Equal(["retries", "number"], diagnostic.Arguments);
        Assert.False(context.Values.ContainsValue("retries"));
    }

    [Fact]
    public void Prepare_OverrideNotInAllowedValues_ReportsDiagnosticAndDoesNotSetValue()
    {
        var pipeline = new PipelineNodeBuilder()
            .WithParameter(x => x
                .WithName("operatingSystem")
                .WithType("string")
                .WithValues(v => v
                    .WithStringLiteralExpressionItem(x => x.WithValue("windows"))
                    .WithStringLiteralExpressionItem(x => x.WithValue("ubuntu"))))
            .Build();

        var context = new CompilationContext();

        _binder.Prepare(
            pipeline,
            new Dictionary<string, string> { ["operatingSystem"] = "solaris" },
            context);

        var diagnostic = Assert.Single(context.Diagnostics.Diagnostics);

        Assert.Equal(DiagnosticDescriptors.ParameterOverrideValueMustBeInValues, diagnostic.Descriptor);
        Assert.Equal(["operatingSystem"], diagnostic.Arguments);
        Assert.False(context.Values.ContainsValue("operatingSystem"));
    }

    [Fact]
    public void Prepare_InvalidOverride_PreservesDefaultValue()
    {
        var pipeline = new PipelineNodeBuilder()
            .WithParameter(x => x
                .WithName("enableDebug")
                .WithType("boolean")
                .WithDefaultValue("true"))
            .Build();

        var context = new CompilationContext();

        _binder.Prepare(
            pipeline,
            new Dictionary<string, string> { ["enableDebug"] = "not-a-bool" },
            context);

        var diagnostic = Assert.Single(context.Diagnostics.Diagnostics);

        Assert.Equal(DiagnosticDescriptors.ParameterOverrideValueTypeMismatch, diagnostic.Descriptor);
        Assert.True(context.Values.TryGetValue("enableDebug", out var value));
        Assert.Equal("true", value);
    }

    [Fact]
    public void Prepare_ValidTypedAndAllowedOverride_IsAccepted()
    {
        var pipeline = new PipelineNodeBuilder()
            .WithParameter(x => x
                .WithName("operatingSystem")
                .WithType("string")
                .WithDefaultValue("ubuntu")
                .WithValues(v => v
                    .WithStringLiteralExpressionItem(x => x.WithValue("windows"))
                    .WithStringLiteralExpressionItem(x => x.WithValue("ubuntu"))))
            .Build();

        var context = new CompilationContext();

        _binder.Prepare(
            pipeline,
            new Dictionary<string, string> { ["operatingSystem"] = "windows" },
            context);

        DiagnosticAssert.Empty(context.Diagnostics);
        Assert.True(context.Values.TryGetValue("operatingSystem", out var value));
        Assert.Equal("windows", value);
    }

    [Fact]
    public void Substitute_UsesPreparedResolvedValues()
    {
        var pipeline = new PipelineNodeBuilder()
            .WithParameter(x => x
                .WithName("serverName")
                .WithDefaultValue("default-host"))
            .WithScriptStep(x => x
                .WithScript(s => s
                    .WithKey("script")
                    .WithInterpolatedStringValue(v => v
                        .WithParameterVariablePart("serverName"))))
            .Build();

        var context = new CompilationContext();

        _binder.Prepare(
            pipeline,
            new Dictionary<string, string> { ["serverName"] = "localhost" },
            context);

        var result = _binder.Substitute(
            pipeline,
            context);

        var script = Assert.IsType<ScriptStepNode>(result.Steps[0]);
        var literal = Assert.IsType<StringLiteralExpressionNode>(script.Script.Value);

        Assert.Equal("localhost", literal.Value);
        DiagnosticAssert.Empty(context.Diagnostics);
    }

    [Fact]
    public void Substitute_NullRoot_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => _binder.Substitute(
                null!,
                new CompilationContext()));
    }

    [Fact]
    public void Substitute_NullContext_Throws()
    {
        var pipeline = PipelineNode.Empty(SourceSpan.Unknown);

        Assert.Throws<ArgumentNullException>(
            () => _binder.Substitute(
                pipeline,
                null!));
    }
}
