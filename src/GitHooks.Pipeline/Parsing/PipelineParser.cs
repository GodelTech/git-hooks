using GitHooks.Pipeline.Ast;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace GitHooks.Pipeline.Parsing;

/// <summary>
/// Parses the pipeline DSL from YAML parser events into a strongly typed AST.
/// </summary>
public sealed class PipelineParser : IPipelineParser
{
    private readonly record struct ParsedStep(string? Script, string? Template, Dictionary<string, string> Parameters);

    private Parser? _parser;
    private string _sourceName = string.Empty;

    /// <inheritdoc/>
    public PipelineNode Parse(string yamlContent, string sourceName)
    {
        if (string.IsNullOrWhiteSpace(yamlContent))
        {
            throw new PipelineException("Pipeline YAML content is empty.");
        }

        if (string.IsNullOrWhiteSpace(sourceName))
        {
            throw new PipelineException("Pipeline source name cannot be empty.");
        }

        try
        {
            _sourceName = sourceName;
            _parser = new Parser(new StringReader(yamlContent));

            _ = Consume<StreamStart>();
            _ = Consume<DocumentStart>();

            var rootStart = Consume<MappingStart>().Start;
            IReadOnlyList<StepNode>? steps = null;

            while (!Accept<MappingEnd>())
            {
                var key = Consume<Scalar>().Value;

                if (string.Equals(key, "steps", StringComparison.Ordinal))
                {
                    steps = ParseSteps();
                    continue;
                }

                Skip();
            }

            _ = Consume<MappingEnd>();
            _ = Consume<DocumentEnd>();
            _ = Consume<StreamEnd>();

            if (steps is null)
            {
                throw new PipelineException($"{_sourceName}: missing required root key 'steps'.");
            }

            return new PipelineNode(rootStart, steps);
        }
        catch (PipelineException)
        {
            throw;
        }
        catch (YamlException ex)
        {
            var innerMessage = ex.InnerException is null ? string.Empty : $" Inner: {ex.InnerException.Message}";
            throw new PipelineException($"{_sourceName}: YAML parse error at line {ex.Start.Line}, column {ex.Start.Column}: {ex.Message}.{innerMessage}", ex);
        }
        catch (Exception ex) when (ex is InvalidOperationException or FormatException)
        {
            throw new PipelineException($"{_sourceName}: {ex.Message}", ex);
        }
        finally
        {
            _parser = null;
            _sourceName = string.Empty;
        }
    }

    /// <summary>
    /// Consumes and returns the next event if it matches the expected event type.
    /// </summary>
    /// <typeparam name="T">The expected event type.</typeparam>
    /// <returns>The consumed event.</returns>
    private T Consume<T>() where T : ParsingEvent
    {
        EnsureParser();
        var parser = _parser!;

        if (parser.TryConsume<T>(out var result))
        {
            return result!;
        }

        var currentType = parser.Current is null ? "<end-of-stream>" : parser.Current.GetType().Name;
        throw new PipelineException($"{_sourceName}: expected {typeof(T).Name} but found {currentType}.");
    }

    /// <summary>
    /// Checks whether the current event is of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The event type to check.</typeparam>
    /// <returns><see langword="true"/> when the current event matches; otherwise <see langword="false"/>.</returns>
    private bool Accept<T>() where T : ParsingEvent
    {
        EnsureParser();
        var parser = _parser!;
        return parser.Accept<T>(out _);
    }

    /// <summary>
    /// Skips the current YAML node, including nested mapping and sequence content.
    /// </summary>
    private void Skip()
    {
        EnsureParser();

        var current = Consume<ParsingEvent>();

        switch (current)
        {
            case MappingStart:
                while (!Accept<MappingEnd>())
                {
                    Skip();
                }

                _ = Consume<MappingEnd>();
                break;

            case SequenceStart:
                while (!Accept<SequenceEnd>())
                {
                    Skip();
                }

                _ = Consume<SequenceEnd>();
                break;

            default:
                break;
        }
    }

    private List<StepNode> ParseSteps()
    {
        var sequenceStart = Consume<SequenceStart>().Start;
        var steps = new List<StepNode>();

        while (!Accept<SequenceEnd>())
        {
            steps.Add(ParseStep());
        }

        _ = Consume<SequenceEnd>();

        if (steps.Count == 0)
        {
            throw new PipelineException($"{_sourceName}: 'steps' sequence at line {sequenceStart.Line}, column {sequenceStart.Column} cannot be empty.");
        }

        return steps;
    }

    private StepNode ParseStep()
    {
        var mappingStart = Consume<MappingStart>();
        var parsedStep = new ParsedStep(null, null, new Dictionary<string, string>(StringComparer.Ordinal));

        while (!Accept<MappingEnd>())
        {
            var key = Consume<Scalar>().Value;

            switch (key)
            {
                case "script":
                case "run":
                    if (parsedStep.Script is not null)
                    {
                        throw new PipelineException($"{_sourceName}: duplicate script/run key at line {mappingStart.Start.Line}, column {mappingStart.Start.Column}.");
                    }

                    parsedStep = parsedStep with { Script = Consume<Scalar>().Value };
                    break;

                case "template":
                    if (parsedStep.Template is not null)
                    {
                        throw new PipelineException($"{_sourceName}: duplicate template key at line {mappingStart.Start.Line}, column {mappingStart.Start.Column}.");
                    }

                    parsedStep = parsedStep with { Template = Consume<Scalar>().Value };
                    break;

                case "parameters":
                    parsedStep = parsedStep with { Parameters = ParseParameters() };
                    break;

                default:
                    Skip();
                    break;
            }
        }

        _ = Consume<MappingEnd>();

        if (parsedStep.Script is not null && parsedStep.Template is not null)
        {
            throw new PipelineException($"{_sourceName}: step at line {mappingStart.Start.Line}, column {mappingStart.Start.Column} cannot declare both 'script' and 'template'.");
        }

        if (parsedStep.Script is not null)
        {
            return new ScriptStepNode(mappingStart.Start, parsedStep.Script);
        }

        if (parsedStep.Template is not null)
        {
            return new TemplateStepNode(mappingStart.Start, parsedStep.Template, parsedStep.Parameters);
        }

        throw new PipelineException($"{_sourceName}: step at line {mappingStart.Start.Line}, column {mappingStart.Start.Column} must contain either 'script'/'run' or 'template'.");
    }

    private Dictionary<string, string> ParseParameters()
    {
        _ = Consume<MappingStart>();
        var parameters = new Dictionary<string, string>(StringComparer.Ordinal);

        while (!Accept<MappingEnd>())
        {
            var key = Consume<Scalar>().Value;
            var value = Consume<Scalar>().Value;
            parameters[key] = value;
        }

        _ = Consume<MappingEnd>();
        return parameters;
    }

    private void EnsureParser()
    {
        if (_parser is null)
        {
            throw new InvalidOperationException("Parser is not initialized.");
        }
    }
}
