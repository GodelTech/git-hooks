using GitHooks.Diagnostics;
using GitHooks.Domain.Ast;

namespace GitHooks.Infrastructure.Yaml.Parsing;

public sealed record YamlParserResult
{
    public PipelineNode? Root { get; init; }

    public IReadOnlyList<Diagnostic> Diagnostics { get; init; }
        = [];
}
