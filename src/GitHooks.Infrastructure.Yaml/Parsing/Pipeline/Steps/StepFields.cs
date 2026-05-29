using GitHooks.Domain.Ast.Expressions;

namespace GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;

internal sealed class StepFields
{
    public ExpressionNode? Script { get; set; }

    public ExpressionNode? Template { get; set; }

    public ExpressionNode? DisplayName { get; set; }

    public ExpressionNode? Condition { get; set; }

    public ExpressionNode? TimeoutInMinutes { get; set; }

    public ExpressionNode? WorkingDirectory { get; set; }

    public Dictionary<string, ExpressionNode> Env { get; }
        = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, ExpressionNode> Parameters { get; }
        = new(StringComparer.OrdinalIgnoreCase);
}
