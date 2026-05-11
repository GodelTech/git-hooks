using GitHooks.Workflow.Infrastructure.Yaml.Expressions;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps.Handlers;

internal sealed class WorkingDirectoryFieldHandler(InterpolationParser parser) : IStepFieldHandler
{
    private readonly InterpolationParser _parser = parser;

    public string Key => "workingDirectory";

    public StepFields Apply(YamlReader reader, StepFields fields)
    {
        return fields with
        {
            WorkingDirectory = _parser.Parse(reader.Read<Scalar>().Value)
        };
    }
}

