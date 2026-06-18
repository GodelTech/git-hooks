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

    public YamlParserResult Parse(
        string yaml,
        SourceDocument sourceDocument)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yaml);
        ArgumentNullException.ThrowIfNull(sourceDocument);

        var context = new ParsingContext(
            yaml,
            sourceDocument);

        try
        {
            _ = context.Cursor.Read<StreamStart>();
            _ = context.Cursor.Read<DocumentStart>();

            var pipeline = _pipelineParser.Parse(context);

            _ = context.Cursor.Read<DocumentEnd>();
            _ = context.Cursor.Read<StreamEnd>();

            return new YamlParserResult
            {
                Root = pipeline,
                Diagnostics = context.Diagnostics
            };
        }
        catch (YamlException exception)
        {
            context.Report(
                Diagnostic.Create(
                    DiagnosticDescriptors.InvalidYaml,
                    context.Cursor.CreateSpan(exception),
                    exception.Message));

            return new YamlParserResult
            {
                Root = null,
                Diagnostics = context.Diagnostics
            };
        }
    }
}
