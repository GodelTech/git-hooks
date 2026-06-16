using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Unknown;
using GitHooks.Domain.Common;
using GitHooks.Infrastructure.Yaml.Parsing.Expressions;
using GitHooks.Infrastructure.Yaml.Parsing.Unknown;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;

internal sealed class StepParser(
    ExpressionParser expressionParser,
    ExpressionMappingParser expressionMappingParser,
    UnknownNodeParser unknownNodeParser)
{
    private readonly ExpressionParser _expressionParser
        = expressionParser ?? throw new ArgumentNullException(nameof(expressionParser));

    private readonly ExpressionMappingParser _expressionMappingParser
        = expressionMappingParser ?? throw new ArgumentNullException(nameof(expressionMappingParser));

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
                        context => ReadExpressionField(key, context));
                    break;

                case "template":
                    step.Template = fields.ReadFirst(
                        key,
                        context,
                        step.Template,
                        context => ReadExpressionField(key, context));
                    break;

                case "displayname":
                    step.DisplayName = fields.ReadFirst(
                        key,
                        context,
                        step.DisplayName,
                        context => ReadExpressionField(key, context));
                    break;

                case "condition":
                    step.Condition = fields.ReadFirst(
                        key,
                        context,
                        step.Condition,
                        context => ReadExpressionField(key, context));
                    break;

                case "timeoutinminutes":
                    step.TimeoutInMinutes = fields.ReadFirst(
                        key,
                        context,
                        step.TimeoutInMinutes,
                        context => ReadExpressionField(key, context));
                    break;

                case "workingdirectory":
                    step.WorkingDirectory = fields.ReadFirst(
                        key,
                        context,
                        step.WorkingDirectory,
                        context => ReadExpressionField(key, context));
                    break;

                case "env":
                    step.Env = fields.ReadFirst(
                        key,
                        context,
                        step.Env,
                        context => _expressionMappingParser.Parse(key, context));
                    break;

                case "parameters":
                    step.Parameters = fields.ReadFirst(
                        key,
                        context,
                        step.Parameters,
                        context => _expressionMappingParser.Parse(key, context));
                    break;

                default:
                    fields.AddUnknownField(key, context);
                    break;
            }
        }

        var end = context.Cursor.Read<MappingEnd>();

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
        if (step.Script is not null &&
            step.Template is not null)
        {
            context.Report(
                Diagnostic.Create(
                    DiagnosticDescriptors.MultipleStepTypes,
                    span));

            return new InvalidStepNode
            {
                Fields = [.. step.GetFields()],
                UnknownFields = unknownFields,
                Span = span
            };
        }

        if (step.Script is not null)
        {
            StepFieldValidation.ValidateScriptStep(step, span, context);

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
            StepFieldValidation.ValidateTemplateStep(step, span, context);

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

    private StringKeyFieldNode<ExpressionNode> ReadExpressionField(
        Scalar key,
        ParsingContext context)
    {
        var value = _expressionParser.Parse(context);

        return new StringKeyFieldNode<ExpressionNode>
        {
            Key = key.Value,
            Value = value,
            Span = context.Cursor.CreateSpan(
                key.Start,
                value.Span)
        };
    }
}
