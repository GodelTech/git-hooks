using GitHooks.Diagnostics;
using GitHooks.Testing.Common;

namespace GitHooks.Validation.Tests;

public sealed class ValidationContextTests
{
    [Fact]
    public void Constructor_NullDiagnostics_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new ValidationContext(null!));

        Assert.Equal(
            "diagnostics",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_CreatesSymbolTable()
    {
        var context = new ValidationContext(
            new DiagnosticBag());

        var symbols = context.Symbols;

        Assert.NotNull(symbols);

        Assert.Same(
            symbols,
            context.Symbols);
    }

    [Fact]
    public void Report_NullDiagnostic_Throws()
    {
        var context = new ValidationContext(
            new DiagnosticBag());

        var exception = Assert.Throws<ArgumentNullException>(
            () => context.Report(null!));

        Assert.Equal(
            "diagnostic",
            exception.ParamName);
    }

    [Fact]
    public void Report_Diagnostic_AddsDiagnostic()
    {
        var diagnostics = new DiagnosticBag();

        var context = new ValidationContext(
            diagnostics);

        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.ParameterNameIsRequired,
            TestSourceSpan.Unknown);

        context.Report(diagnostic);

        var actual = Assert.Single(diagnostics.Diagnostics);

        Assert.Same(
            diagnostic,
            actual);
    }
}
