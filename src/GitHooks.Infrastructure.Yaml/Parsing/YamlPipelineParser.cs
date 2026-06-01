using GitHooks.Diagnostics;
using GitHooks.Infrastructure.Yaml.Parsing.Exceptions;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing;

internal sealed class YamlPipelineParser(
    PipelineParser pipelineParser)
{
    private readonly PipelineParser _pipelineParser
        = pipelineParser ?? throw new ArgumentNullException(nameof(pipelineParser));

    public YamlParserResult Parse(string yaml, string sourceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yaml);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);

        var diagnostics = new DiagnosticBag();

        var cursor = YamlParserCursor.Create(
                yaml,
                sourceName);

        try
        {
            cursor = YamlParserCursor.Create(
                yaml,
                sourceName);

            _ = cursor.Read<StreamStart>();
            _ = cursor.Read<DocumentStart>();

            var pipeline = _pipelineParser.Parse(cursor);

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
            diagnostics.Report(
                new Diagnostic
                {
                    Code = DiagnosticCode.InvalidYaml,
                    Message = exception.Message,
                    Severity = DiagnosticSeverity.Error,
                    Span = cursor.CreateSpan(exception)
                });

            return new YamlParserResult
            {
                Root = null,
                Diagnostics = diagnostics.Diagnostics
            };
        }
        catch (YamlPipelineParsingException exception)
        {
            diagnostics.Report(
                new Diagnostic
                {
                    Code = DiagnosticCode.InvalidYaml,
                    Message = exception.Message,
                    Severity = DiagnosticSeverity.Error,
                    Span = exception.Span
                });

            return new YamlParserResult
            {
                Root = null,
                Diagnostics = diagnostics.Diagnostics
            };
        }
    }
}
