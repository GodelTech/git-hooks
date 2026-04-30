using GitHooks.Workflow.Infrastructure.Yaml.Expressions;

namespace GitHooks.Workflow.Infrastructure.Yaml.Steps.Handlers;

internal sealed class ParametersFieldHandler(InterpolationParser parser)
    : MapFieldHandler(parser)
{
    public override string Key => "parameters";

    public override StepFields Apply(YamlReader reader, StepFields fields)
    {
        return fields with { Parameters = ReadMap(reader) };
    }
}
