using GitHooks.Diagnostics;
using GitHooks.Domain.Ast;

namespace GitHooks.Infrastructure.Yaml.Parsing.Pipeline;

internal sealed class PipelineParser
{
    public PipelineNode Parse(
        YamlParserCursor cursor,
        DiagnosticBag diagnostics)
    {
        throw new NotImplementedException("Pipeline parsing is not implemented yet.");
    }
}
