using GitHooks.Workflow.Infrastructure.Yaml.Expressions;

namespace GitHooks.Workflow.Infrastructure.Yaml.Steps.Handlers;

internal sealed class EnvFieldHandler(InterpolationParser parser)
    : MapFieldHandler(parser)
{
    public override string Key => "env";

    public override StepFields Apply(YamlReader reader, StepFields fields)
    {
        return fields with { Env = ReadMap(reader) };
    }
}
