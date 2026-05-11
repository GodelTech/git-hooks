using GitHooks.Workflow.Infrastructure.Yaml.Expressions;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps.Handlers;

internal sealed class ConditionFieldHandler(ExpressionParser parser) : IStepFieldHandler
{
    private readonly ExpressionParser _parser = parser;

    public string Key => "condition";

    public StepFields Apply(YamlReader reader, StepFields fields)
    {
        return fields with
        {
            Condition = _parser.Parse(reader.Read<Scalar>().Value)
        };
    }
}
