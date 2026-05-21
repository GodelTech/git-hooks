using GitHooks.Diagnostics;
using GitHooks.Domain.Ast;
using GitHooks.Domain.Common;

using YamlDotNet.Core;

namespace GitHooks.Infrastructure.Yaml.Parsing;

public sealed class YamlPipelineParser
{
    private readonly DiagnosticBag _diagnostics = new();

    public YamlParserResult Parse(
        string yaml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yaml);

        try
        {
            // TODO:
            // Parse YAML into AST.
            var pipeline = new PipelineNode
            {
                Span = SourceSpan.Unknown,

                Parameters = [],

                Steps = []
            };

            return new YamlParserResult
            {
                Root = pipeline,

                Diagnostics = _diagnostics.Diagnostics
            };
        }
        catch (YamlException exception)
        {
            _diagnostics.Report(
                new Diagnostic
                {
                    Code = DiagnosticCodes.InvalidYaml,
                    Message = exception.Message,
                    Severity = DiagnosticSeverity.Error,
                    Span = SourceSpan.Unknown
                });

            return new YamlParserResult
            {
                Root = null,

                Diagnostics = _diagnostics.Diagnostics
            };
        }
    }
}
