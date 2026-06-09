using GitHooks.Diagnostics;
using GitHooks.Domain.Common;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing;

// todo: recoverable parsing.
internal sealed class YamlPipelineParser(
    PipelineParser pipelineParser)
{
    private readonly PipelineParser _pipelineParser
        = pipelineParser ?? throw new ArgumentNullException(nameof(pipelineParser));

    public YamlParserResult Parse(string yaml, SourceDocument sourceDocument)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yaml);
        ArgumentNullException.ThrowIfNull(sourceDocument);

        var diagnostics = new DiagnosticBag();

        var cursor = YamlParserCursor.Create(
            yaml,
            sourceDocument);

        try
        {
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
                DiagnosticDescriptors.InvalidYaml,
                cursor.CreateSpan(exception),
                exception.Message);

            return new YamlParserResult
            {
                Root = null,
                Diagnostics = diagnostics.Diagnostics
            };
        }
    }
}
