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
                        Descriptor = new DiagnosticDescriptor
                        {
                            Code = DiagnosticCode.Create(1),
                            Severity = DiagnosticSeverity.Error,
                            MessageFormat = "Error"
                        },
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
                        Descriptor = new DiagnosticDescriptor
                        {
                            Code = DiagnosticCode.Create(2),
                            Severity = DiagnosticSeverity.Warning,
                            MessageFormat = "Warning"
                        },
                        Span = SourceSpan.Unknown
                    }
                ]
            };

        Assert.False(result.HasErrors);
    }
}
