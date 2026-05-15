using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Infrastructure.Yaml.Expressions;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps.Handlers;

internal abstract class MapFieldHandlerBase : IStepFieldHandler
{
    private readonly InterpolationParser _parser;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Protected constructor is required for inheritance")]
    protected MapFieldHandlerBase(InterpolationParser parser)
    {
        _parser = parser;
    }

    public abstract string Key { get; }

    public abstract StepFields Apply(YamlReader reader, StepFields fields);

    protected IReadOnlyDictionary<string, InterpolatedStringNode> ReadMap(YamlReader reader)
    {
        _ = reader.Read<MappingStart>();

        var result = new Dictionary<string, InterpolatedStringNode>(StringComparer.Ordinal);

        while (!reader.Is<MappingEnd>())
        {
            var key = reader.Read<Scalar>().Value;
            var value = reader.Read<Scalar>().Value;

            result[key] = _parser.Parse(value);
        }

        _ = reader.Read<MappingEnd>();

        return result;
    }
}
