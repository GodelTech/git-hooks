using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Mappings;

namespace GitHooks.Infrastructure.Yaml.Parsing;

public sealed record YamlParserResult
{
    public required PipelineNode? Root { get; init; }

    public required IReadOnlyList<Diagnostic> Diagnostics { get; init; }

    public bool HasErrors
        => Diagnostics.Any(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
}
