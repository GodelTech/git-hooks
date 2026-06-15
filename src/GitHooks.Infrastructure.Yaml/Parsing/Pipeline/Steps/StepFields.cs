using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Syntax;

namespace GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;

internal sealed class StepFields
{
    public StringKeyFieldNode<ExpressionNode>? Script { get; set; }

    public StringKeyFieldNode<ExpressionNode>? Template { get; set; }

    public StringKeyFieldNode<ExpressionNode>? DisplayName { get; set; }

    public StringKeyFieldNode<ExpressionNode>? Condition { get; set; }

    public StringKeyFieldNode<ExpressionNode>? TimeoutInMinutes { get; set; }

    public StringKeyFieldNode<ExpressionNode>? WorkingDirectory { get; set; }

    public MappingFieldNode<StringKeyFieldNode<ExpressionNode>>? Env { get; set; }

    public MappingFieldNode<StringKeyFieldNode<ExpressionNode>>? Parameters { get; set; }

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

        if (Env is not null)
        {
            yield return StepFieldNames.Env;
        }

        if (Parameters is not null)
        {
            yield return StepFieldNames.Parameters;
        }
    }
}
