using YamlDotNet.Core;

namespace GitHooks.Pipeline.Parsing;

public interface IYamlParserFactory
{
    public IParser Create(string yaml);
}
