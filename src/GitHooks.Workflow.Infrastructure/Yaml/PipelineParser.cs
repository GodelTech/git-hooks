using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Parsing;
using GitHooks.Workflow.Infrastructure.Yaml.Pipeline;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml;

internal sealed class PipelineParser(PipelineRootParser pipelineRootParser)
        : IPipelineParser
{
    private readonly PipelineRootParser _pipelineRootParser = pipelineRootParser;

    public PipelineNode Parse(string content, string sourceName)
    {
        var reader = YamlReader.Create(content, sourceName);

        reader.Require<StreamStart>();
        reader.Require<DocumentStart>();

        var result = _pipelineRootParser.Parse(reader);

        reader.Require<DocumentEnd>();
        reader.Require<StreamEnd>();

        return result;
    }
}
