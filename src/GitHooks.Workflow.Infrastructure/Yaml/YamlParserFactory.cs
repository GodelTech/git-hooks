using GitHooks.Workflow.Domain.Model;

using YamlDotNet.Core;

namespace GitHooks.Workflow.Infrastructure.Yaml;

internal sealed class YamlParserFactory : IYamlParserFactory
{
    public YamlReader Create(string yaml, string sourceName)
    {
        var parser = new Parser(new StringReader(yaml));
        var source = new SourceRef(sourceName);

        return new YamlReader(parser, source);
    }
}
