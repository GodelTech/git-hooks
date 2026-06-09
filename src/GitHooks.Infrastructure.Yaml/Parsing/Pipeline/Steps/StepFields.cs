using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Syntax;

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

    public IEnumerable<string> GetPresentFields()
    {
        if (Script is not null)
        {
            yield return StepFieldNames.Script;
        }

        if (Template is not null)
        {
            yield return StepFieldNames.Template;
        }

        if (DisplayName is not null)
        {
            yield return StepFieldNames.DisplayName;
        }

        if (Condition is not null)
        {
            yield return StepFieldNames.Condition;
        }

        if (TimeoutInMinutes is not null)
        {
            yield return StepFieldNames.TimeoutInMinutes;
        }

        if (WorkingDirectory is not null)
        {
            yield return StepFieldNames.WorkingDirectory;
        }

        if (Env.Count is > 0)
        {
            yield return StepFieldNames.Env;
        }

        if (Parameters.Count is > 0)
        {
            yield return StepFieldNames.Parameters;
        }
    }
}
