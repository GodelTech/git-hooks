using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps.Builders;

internal sealed class ScriptStepNodeBuilder : StepNodeBuilderBase
{
    private static readonly IReadOnlySet<string> AllowedFields = new HashSet<string>
    {
        "script",
        "displayName",
        "condition",
        "timeoutInMinutes",
        "workingDirectory",
        "env"
    };

    /// <inheritdoc />
    public override bool CanBuild(StepFields fields)
    {
        return fields.Script is not null;
    }

    /// <inheritdoc />
    protected override string StepType => "Script";

    /// <inheritdoc />
    protected override IReadOnlySet<string> GetAllowedFields()
    {
        return AllowedFields;
    }

    /// <inheritdoc />
    protected override StepNode Create(StepFields fields, SourceSpan span)
    {
        var script = fields.Script ?? throw new InvalidOperationException("Script step builder requires a script value.");

        return new ScriptStepNode(script, fields.UnknownFields, span)
        {
            DisplayName = fields.DisplayName,
            Condition = fields.Condition,
            TimeoutInMinutes = fields.TimeoutInMinutes,
            WorkingDirectory = fields.WorkingDirectory,
            Env = fields.Env
        };
    }
}
