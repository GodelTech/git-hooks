using GitHooks.Workflow.Application.Ast.Expressions;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps;

internal sealed record StepFields
{
    public InterpolatedStringNode? Script { get; init; }

    public string? Template { get; init; }

    public string? DisplayName { get; init; }

    public ExpressionNode? Condition { get; init; }

    public int? TimeoutInMinutes { get; init; }

    public InterpolatedStringNode? WorkingDirectory { get; init; }

    public IReadOnlyDictionary<string, InterpolatedStringNode> Env { get; init; }
        = new Dictionary<string, InterpolatedStringNode>();

    public IReadOnlyDictionary<string, InterpolatedStringNode> Parameters { get; init; }
        = new Dictionary<string, InterpolatedStringNode>();
}
