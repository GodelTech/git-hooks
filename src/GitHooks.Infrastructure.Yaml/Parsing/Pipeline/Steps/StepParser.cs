using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Common;
using GitHooks.Infrastructure.Yaml.Parsing.Fields;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Parsing.Pipeline.Steps;

internal sealed class StepParser(
    FieldParser fieldParser,
    FieldValueParser fieldValueParser)
{
    private readonly FieldParser _fieldParser
        = fieldParser ?? throw new ArgumentNullException(nameof(fieldParser));

    private readonly FieldValueParser _fieldValueParser
        = fieldValueParser ?? throw new ArgumentNullException(nameof(fieldValueParser));

    public StepNode Parse(ParsingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var start = context.Cursor.Read<MappingStart>();

        var fieldTracker = new FieldTracker(_fieldValueParser);

        var step = new StepFields();

        while (!context.Cursor.Is<MappingEnd>())
        {
            if (!context.Cursor.Is<Scalar>())
            {
                fieldTracker.AddUnknownField(context);

                continue;
            }

            var key = context.Cursor.Read<Scalar>();

            switch (key.Value.ToLowerInvariant())
            {
                case "script":
                    step.Script = fieldTracker.ReadFirst(
                        key,
                        context,
                        step.Script,
                        context => _fieldParser.ParseStringKeyField(key, context));
                    break;

                case "template":
                    step.Template = fieldTracker.ReadFirst(
                        key,
                        context,
                        step.Template,
                        context => _fieldParser.ParseStringKeyField(key, context));
                    break;

                case "displayname":
                    step.DisplayName = fieldTracker.ReadFirst(
                        key,
                        context,
                        step.DisplayName,
                        context => _fieldParser.ParseStringKeyField(key, context));
                    break;

                case "condition":
                    step.Condition = fieldTracker.ReadFirst(
                        key,
                        context,
                        step.Condition,
                        context => _fieldParser.ParseStringKeyField(key, context));
                    break;

                case "timeoutinminutes":
                    step.TimeoutInMinutes = fieldTracker.ReadFirst(
                        key,
                        context,
                        step.TimeoutInMinutes,
                        context => _fieldParser.ParseStringKeyField(key, context));
                    break;

                case "workingdirectory":
                    step.WorkingDirectory = fieldTracker.ReadFirst(
                        key,
                        context,
                        step.WorkingDirectory,
                        context => _fieldParser.ParseStringKeyField(key, context));
                    break;

                case "env":
                    step.Env = fieldTracker.ReadFirst(
                        key,
                        context,
                        step.Env,
                        context => _fieldParser.ParseMappingField(key, context));
                    break;

                case "parameters":
                    step.Parameters = fieldTracker.ReadFirst(
                        key,
                        context,
                        step.Parameters,
                        context => _fieldParser.ParseMappingField(key, context));
                    break;

                default:
                    fieldTracker.AddUnknownField(key, context);
                    break;
            }
        }

        var end = context.Cursor.Read<MappingEnd>();

        var span = context.Cursor.CreateSpan(start, end);

        return BuildStep(
            step,
            fieldTracker.GetUnknownFields(),
            span,
            context);
    }

    private static StepNode BuildStep(
        StepFields step,
        IReadOnlyList<FieldNode> unknownFields,
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
                UnknownFields = [.. unknownFields],
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
                UnknownFields = [.. unknownFields],
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
                UnknownFields = [.. unknownFields],
                Span = span
            };
        }

        throw context.Cursor.CreateException(
            "Step must contain exactly one step type field");
    }
}
