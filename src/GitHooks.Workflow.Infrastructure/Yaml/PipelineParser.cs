using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Parsing;
using GitHooks.Workflow.Infrastructure.Yaml.Sections;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml;

internal sealed class PipelineParser(IYamlParserFactory factory, PipelineRootParser pipelineRootParser)
        : IPipelineParser
{
    private readonly IYamlParserFactory _factory = factory;
    private readonly PipelineRootParser _pipelineRootParser = pipelineRootParser;

    public PipelineNode Parse(string content, string sourceName)
    {
        var reader = _factory.Create(content, sourceName);

        reader.Require<StreamStart>();
        reader.Require<DocumentStart>();

        var result = _pipelineRootParser.Parse(reader);

        reader.Require<DocumentEnd>();
        reader.Require<StreamEnd>();

        return result;
    }
}
