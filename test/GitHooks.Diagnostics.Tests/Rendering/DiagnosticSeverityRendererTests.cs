using GitHooks.Diagnostics.Rendering;

namespace GitHooks.Diagnostics.Tests.Rendering;

public sealed class DiagnosticSeverityRendererTests
{
    [Fact]
    public void Render_Info_ReturnsInfo()
    {
        var result = DiagnosticSeverityRenderer.Render(DiagnosticSeverity.Info);

        Assert.Equal(
            "info",
            result);
    }

    [Fact]
    public void Render_Warning_ReturnsWarning()
    {
        var result = DiagnosticSeverityRenderer.Render(DiagnosticSeverity.Warning);

        Assert.Equal(
            "warning",
            result);
    }

    [Fact]
    public void Render_Error_ReturnsError()
    {
        var result = DiagnosticSeverityRenderer.Render(DiagnosticSeverity.Error);

        Assert.Equal(
            "error",
            result);
    }

    [Fact]
    public void Render_UnknownSeverity_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => DiagnosticSeverityRenderer.Render(
                (DiagnosticSeverity)999));
    }
}
