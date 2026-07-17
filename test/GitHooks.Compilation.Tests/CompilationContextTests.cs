namespace GitHooks.Compilation.Tests;

public sealed class CompilationContextTests
{
    [Fact]
    public void Constructor_WithoutOptions_CreatesDefaults()
    {
        var context = new CompilationContext();

        Assert.NotNull(context.Diagnostics);
        Assert.NotNull(context.Options);
    }

    [Fact]
    public void Constructor_WithOptions_InitializesProperties()
    {
        var options = new CompilationOptions();

        var context = new CompilationContext(options);

        Assert.NotNull(context.Diagnostics);

        Assert.Same(
            options,
            context.Options);
    }

    [Fact]
    public void Constructor_NullOptions_CreatesDefaults()
    {
        var context = new CompilationContext(null);

        Assert.NotNull(context.Diagnostics);
        Assert.NotNull(context.Options);
    }

    [Fact]
    public void Constructor_CreatesEmptyDiagnosticBag()
    {
        var context = new CompilationContext();

        Assert.Empty(context.Diagnostics.Diagnostics);
    }
}
