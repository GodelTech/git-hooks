using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Unknown;
using GitHooks.Domain.Common;
using GitHooks.Infrastructure.Yaml.Parsing.Expressions;
using GitHooks.Infrastructure.Yaml.Parsing.Unknown;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;

internal sealed class StepParser(
    ExpressionParser expressionParser,
    UnknownNodeParser unknownNodeParser)
{
    private readonly ExpressionParser _expressionParser
        = expressionParser ?? throw new ArgumentNullException(nameof(expressionParser));

    private readonly UnknownNodeParser _unknownNodeParser
        = unknownNodeParser ?? throw new ArgumentNullException(nameof(unknownNodeParser));

    public StepNode Parse(YamlParserCursor cursor)
    {
        ArgumentNullException.ThrowIfNull(cursor);

        var start = cursor.Read<MappingStart>();

        var fields = new MappingFields(_unknownNodeParser);

        var step = new StepFields();

        while (!cursor.Is<MappingEnd>())
        {
            if (!cursor.Is<Scalar>())
            {
                fields.AddUnknownField(cursor);

                continue;
            }

            var key = cursor.Read<Scalar>();

            switch (key.Value.ToLowerInvariant())
            {
                case "script":
                    fields.MarkSeen(key, cursor);
                    step.Script = _expressionParser.Parse(cursor);
                    break;

                case "template":
                    fields.MarkSeen(key, cursor);
                    step.Template = _expressionParser.Parse(cursor);
                    break;

                case "displayname":
                    fields.MarkSeen(key, cursor);
                    step.DisplayName = _expressionParser.Parse(cursor);
                    break;

                case "condition":
                    fields.MarkSeen(key, cursor);
                    step.Condition = _expressionParser.Parse(cursor);
                    break;

                case "timeoutinminutes":
                    fields.MarkSeen(key, cursor);
                    step.TimeoutInMinutes = _expressionParser.Parse(cursor);
                    break;

                case "workingdirectory":
                    fields.MarkSeen(key, cursor);
                    step.WorkingDirectory = _expressionParser.Parse(cursor);
                    break;

                case "env":
                    fields.MarkSeen(key, cursor);
                    ParseExpressionDictionary(step.Env, cursor);
                    break;

                case "parameters":
                    fields.MarkSeen(key, cursor);
                    ParseExpressionDictionary(step.Parameters, cursor);
                    break;

                default:
                    fields.AddUnknownField(key, cursor);
                    break;
            }
        }

        var end = cursor.Read<MappingEnd>();

        if (step.Script is not null &&
            step.Template is not null)
        {
            throw cursor.CreateException(
                "Step cannot contain multiple step type fields");
        }

        var span = cursor.CreateSpan(start, end);

        return BuildStep(
            step,
            fields.GetUnknownFields(),
            span,
            cursor);
    }

    private static StepNode BuildStep(
        StepFields step,
        IReadOnlyList<UnknownFieldNode> unknownFields,
        SourceSpan span,
        YamlParserCursor cursor)
    {
        if (step.Script is not null)
        {
            StepFieldValidation.ValidateScriptStep(step, cursor);

            return new ScriptStepNode
            {
                Script = step.Script,
                DisplayName = step.DisplayName,
                Condition = step.Condition,
                TimeoutInMinutes = step.TimeoutInMinutes,
                WorkingDirectory = step.WorkingDirectory,
                Env = step.Env,
                UnknownFields = unknownFields,
                Span = span
            };
        }

        if (step.Template is not null)
        {
            StepFieldValidation.ValidateTemplateStep(step, cursor);

            return new TemplateStepNode
            {
                Template = step.Template,
                Parameters = step.Parameters,
                UnknownFields = unknownFields,
                Span = span
            };
        }

        throw cursor.CreateException(
            "Step must contain exactly one step type field");
    }

    private void ParseExpressionDictionary(Dictionary<string, ExpressionNode> values, YamlParserCursor cursor)
    {
        _ = cursor.Read<MappingStart>();

        var fields = new FieldTracker();

        while (!cursor.Is<MappingEnd>())
        {
            var key = cursor.Read<Scalar>();

            fields.MarkSeen(key, cursor);

            var value = _expressionParser.Parse(cursor);

            values.Add(key.Value, value);
        }

        _ = cursor.Read<MappingEnd>();
    }
}
