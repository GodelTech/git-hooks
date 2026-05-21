using GitHooks.Diagnostics;
using GitHooks.Domain.Common;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing;

internal sealed class YamlPipelineParser(
    PipelineParser pipelineParser)
{
    private readonly PipelineParser _pipelineParser = pipelineParser;

    public YamlParserResult Parse(
        string yaml,
        string sourceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yaml);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);

        YamlParserCursor? cursor = null;
        var diagnostics = new DiagnosticBag();

        try
        {
            cursor = YamlParserCursor.Create(
                yaml,
                sourceName);

            _ = cursor.Read<StreamStart>();
            _ = cursor.Read<DocumentStart>();

            var pipeline = _pipelineParser.Parse(cursor, diagnostics);

            _ = cursor.Read<DocumentEnd>();
            _ = cursor.Read<StreamEnd>();

            return new YamlParserResult
            {
                Root = pipeline,
                Diagnostics = diagnostics.Diagnostics
            };
        }
        catch (YamlException exception)
        {
            var span = cursor is null
                ? SourceSpan.Unknown
                : cursor.CreateSpan(exception);

            diagnostics.Report(
                new Diagnostic
                {
                    Code = DiagnosticCodes.InvalidYaml,
                    Message = exception.Message,
                    Severity = DiagnosticSeverity.Error,
                    Span = span
                });

            return new YamlParserResult
            {
                Root = null,
                Diagnostics = diagnostics.Diagnostics
            };
        }
    }
}
