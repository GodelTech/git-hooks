using GitHooks.Workflow.Infrastructure.Yaml.Expressions;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps.Handlers;

internal sealed class ScriptFieldHandler(InterpolationParser parser) : IStepFieldHandler
{
    private readonly InterpolationParser _parser = parser;

    public string Key => "script";

    public StepFields Apply(YamlReader reader, StepFields fields)
    {
        return fields with
        {
            Script = _parser.Parse(reader.Read<Scalar>().Value)
        };
    }
}

