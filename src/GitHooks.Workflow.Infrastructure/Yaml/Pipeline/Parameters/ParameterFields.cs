using GitHooks.Workflow.Application.Ast;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Parameters;

internal sealed record ParameterFields
{
    public string? Name { get; init; }

    public string? DisplayName { get; init; }

    public ParameterType Type { get; init; }
        = ParameterType.Text;

    public string? DefaultValue { get; init; }

    public IReadOnlyList<string> Values { get; init; }
        = [];
}
