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
                    step.Script = fields.ReadFirst(
                        key,
                        context,
                        step.Script,
                        _expressionParser.Parse);
                    break;

                case "template":
                    step.Template = fields.ReadFirst(
                        key,
                        context,
                        step.Template,
                        _expressionParser.Parse);
                    break;

                case "displayname":
                    step.DisplayName = fields.ReadFirst(
                        key,
                        context,
                        step.DisplayName,
                        _expressionParser.Parse);
                    break;

                case "condition":
                    step.Condition = fields.ReadFirst(
                        key,
                        context,
                        step.Condition,
                        _expressionParser.Parse);
                    break;

                case "timeoutinminutes":
                    step.TimeoutInMinutes = fields.ReadFirst(
                        key,
                        context,
                        step.TimeoutInMinutes,
                        _expressionParser.Parse);
                    break;

                case "workingdirectory":
                    step.WorkingDirectory = fields.ReadFirst(
                        key,
                        context,
                        step.WorkingDirectory,
                        _expressionParser.Parse);
                    break;

                case "env":
                    step.Env = fields.ReadFirst(
                        key,
                        context,
                        step.Env,
                        ParseExpressionDictionary);
                    break;

                case "parameters":
                    step.Parameters = fields.ReadFirst(
                        key,
                        context,
                        step.Parameters,
                        ParseExpressionDictionary);
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

    private Dictionary<string, ExpressionNode> ParseExpressionDictionary(ParsingContext context)
    {
        var values = new Dictionary<string, ExpressionNode>(
            StringComparer.OrdinalIgnoreCase);

        _ = context.Cursor.Read<MappingStart>();

        var fields = new MappingFields(_unknownNodeParser);

        while (!context.Cursor.Is<MappingEnd>())
        {
            if (!context.Cursor.Is<Scalar>())
            {
                // todo: where we are assign UknownFieldNode
                fields.AddUnknownField(context);

                continue;
            }

            var key = context.Cursor.Read<Scalar>();

            _ = values.TryGetValue(
                key.Value,
                out var currentValue);

            var value = fields.ReadFirst(
                key,
                context,
                currentValue,
                _expressionParser.Parse);

            if (value is not null)
            {
                values[key.Value] = value;
            }
        }

        _ = context.Cursor.Read<MappingEnd>();

        return values;
    }
}
