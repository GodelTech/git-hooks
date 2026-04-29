using GitHooks.Workflow.Application.Ast;

using YamlDotNet.Core.Events;

namespace GitHooks.Workflow.Infrastructure.Yaml.Sections;

internal sealed class StepParser
{
#pragma warning disable CA1822 // Mark members as static
    public StepNode Parse(YamlReader reader)
#pragma warning restore CA1822 // Mark members as static
    {
        var start = reader.Read<MappingStart>();

        string? script = null;
        string? displayName = null;

        while (!reader.Is<MappingEnd>())
        {
            var key = reader.Read<Scalar>().Value;

            switch (key)
            {
                case "script":
                    {
                        var scalar = reader.Read<Scalar>();
                        script = scalar.Value;
                        break;
                    }

                case "displayName":
                    {
                        var scalar = reader.Read<Scalar>();
                        displayName = scalar.Value;
                        break;
                    }

                default:
                    reader.SkipNode();
                    break;
            }
        }

        var end = reader.Read<MappingEnd>();

        if (script is null)
        {
            throw new YamlParseException(
                "Step must contain 'script'",
                reader.SpanOf(start, end)
            );
        }

        return new ScriptStepNode(
            reader.SpanOf(start, end),
            script
        )
        {
            DisplayName = displayName
        };
    }
}
