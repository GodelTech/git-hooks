using GitHooks.Compilation.Parsing;
using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Infrastructure.Yaml.Parsing.Pipeline;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing;

internal sealed class YamlPipelineParser(
    PipelineParser pipelineParser)
    : IPipelineParser
{
    private readonly PipelineParser _pipelineParser
        = pipelineParser ?? throw new ArgumentNullException(nameof(pipelineParser));

    public PipelineNode Parse(
        ParsingContext context,
        DiagnosticBag diagnostics)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(diagnostics);

        var parserContext = new YamlParserContext(context, diagnostics);

        try
        {
            _ = parserContext.Cursor.Read<StreamStart>();
            _ = parserContext.Cursor.Read<DocumentStart>();

            var pipeline = _pipelineParser.Parse(parserContext);

            _ = parserContext.Cursor.Read<DocumentEnd>();
            _ = parserContext.Cursor.Read<StreamEnd>();

            return pipeline;
        }
        catch (YamlException exception)
        {
            var span = parserContext.Cursor.CreateSpan(exception);

            parserContext.Diagnostics.Report(
                Diagnostic.Create(
                    DiagnosticDescriptors.InvalidYaml,
                    span,
                    exception.Message));

            return PipelineNode.Empty(span);
        }
    }
}
