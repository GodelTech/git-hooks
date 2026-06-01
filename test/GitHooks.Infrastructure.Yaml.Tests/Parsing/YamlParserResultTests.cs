using GitHooks.Diagnostics;
using GitHooks.Domain.Common;
using GitHooks.Infrastructure.Yaml.Parsing;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing;

public sealed class YamlParserResultTests
{
    [Fact]
    public void HasErrors_WithErrorDiagnostic_ReturnsTrue()
    {
        var result =
            new YamlParserResult
            {
                Root = null,
                Diagnostics =
                [
                    new Diagnostic
                    {
                        Code = DiagnosticCode.InvalidYaml,
                        Message = "Error",
                        Severity = DiagnosticSeverity.Error,
                        Span = SourceSpan.Unknown
                    }
                ]
            };

        Assert.True(result.HasErrors);
    }

    [Fact]
    public void HasErrors_WithoutErrorDiagnostic_ReturnsFalse()
    {
        var result =
            new YamlParserResult
            {
                Root = null,
                Diagnostics =
                [
                    new Diagnostic
                    {
                        Code = DiagnosticCode.InvalidYaml,
                        Message = "Warning",
                        Severity = DiagnosticSeverity.Warning,
                        Span = SourceSpan.Unknown
                    }
                ]
            };

        Assert.False(result.HasErrors);
    }
}
