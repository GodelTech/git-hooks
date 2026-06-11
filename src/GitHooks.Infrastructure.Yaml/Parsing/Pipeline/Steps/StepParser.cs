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

    public StepNode Parse(ParsingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var start = context.Cursor.Read<MappingStart>();

        var fields = new MappingFields(_unknownNodeParser);

        var step = new StepFields();

        while (!context.Cursor.Is<MappingEnd>())
        {
            if (!context.Cursor.Is<Scalar>())
            {
                fields.AddUnknownField(context);

                continue;
            }

            var key = context.Cursor.Read<Scalar>();

            switch (key.Value.ToLowerInvariant())
            {
                case "script":
                    fields.MarkSeen(key, context);
                    step.Script = _expressionParser.Parse(context);
                    break;

                case "template":
                    fields.MarkSeen(key, context);
                    step.Template = _expressionParser.Parse(context);
                    break;

                case "displayname":
                    fields.MarkSeen(key, context);
                    step.DisplayName = _expressionParser.Parse(context);
                    break;

                case "condition":
                    fields.MarkSeen(key, context);
                    step.Condition = _expressionParser.Parse(context);
                    break;

                case "timeoutinminutes":
                    fields.MarkSeen(key, context);
                    step.TimeoutInMinutes = _expressionParser.Parse(context);
                    break;

                case "workingdirectory":
                    fields.MarkSeen(key, context);
                    step.WorkingDirectory = _expressionParser.Parse(context);
                    break;

                case "env":
                    fields.MarkSeen(key, context);
                    ParseExpressionDictionary(step.Env, context);
                    break;

                case "parameters":
                    fields.MarkSeen(key, context);
                    ParseExpressionDictionary(step.Parameters, context);
                    break;

                default:
                    fields.AddUnknownField(key, context);
                    break;
            }
        }

        var end = context.Cursor.Read<MappingEnd>();

        if (step.Script is not null &&
            step.Template is not null)
        {
            throw context.Cursor.CreateException(
                "Step cannot contain multiple step type fields");
        }

        var span = context.Cursor.CreateSpan(start, end);

        return BuildStep(
            step,
            fields.GetUnknownFields(),
            span,
            context);
    }

    private static StepNode BuildStep(
        StepFields step,
        IReadOnlyList<UnknownFieldNode> unknownFields,
        SourceSpan span,
        ParsingContext context)
    {
        if (step.Script is not null)
        {
            StepFieldValidation.ValidateScriptStep(step, context);

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
            StepFieldValidation.ValidateTemplateStep(step, context);

            return new TemplateStepNode
            {
                Template = step.Template,
                Parameters = step.Parameters,
                UnknownFields = unknownFields,
                Span = span
            };
        }

        throw context.Cursor.CreateException(
            "Step must contain exactly one step type field");
    }

    private void ParseExpressionDictionary(Dictionary<string, ExpressionNode> values, ParsingContext context)
    {
        _ = context.Cursor.Read<MappingStart>();

        var fields = new FieldTracker();

        while (!context.Cursor.Is<MappingEnd>())
        {
            var key = context.Cursor.Read<Scalar>();

            fields.MarkSeen(key, context);

            var value = _expressionParser.Parse(context);

            values.Add(key.Value, value);
        }

        _ = context.Cursor.Read<MappingEnd>();
    }
}
