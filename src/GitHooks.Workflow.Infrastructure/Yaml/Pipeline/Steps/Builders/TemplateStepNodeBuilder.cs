using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps.Builders;

internal sealed class TemplateStepNodeBuilder : StepNodeBuilderBase
{
    private static readonly IReadOnlySet<string> s_allowedFields = new HashSet<string>
    {
        "template",
        "parameters"
    };

    /// <inheritdoc />
    protected override string StepType => "Template";

    /// <inheritdoc />
    public override bool CanBuild(StepFields fields)
    {
        return fields.Template is not null;
    }

    /// <inheritdoc />
    protected override IReadOnlySet<string> GetAllowedFields()
    {
        return s_allowedFields;
    }

    /// <inheritdoc />
    protected override StepNode Create(StepFields fields, SourceSpan span)
    {
        var template = fields.Template ?? throw new InvalidOperationException($"{StepType} step builder requires a template value.");

        return new TemplateStepNode(template, fields.Parameters, fields.UnknownFields, span);
    }
}
