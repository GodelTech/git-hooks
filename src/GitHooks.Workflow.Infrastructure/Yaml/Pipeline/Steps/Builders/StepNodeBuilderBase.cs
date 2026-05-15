using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Domain.Model;
using GitHooks.Workflow.Infrastructure.Yaml.Exceptions;

namespace GitHooks.Workflow.Infrastructure.Yaml.Pipeline.Steps.Builders;

/// <summary>
/// Abstract base class for step node builders using the Template Method pattern.
/// Validates that no fields outside the allowed set are present before delegating
/// to the subclass to construct the concrete <see cref="StepNode"/>.
/// </summary>
/// <remarks>
/// Using an allowed (whitelist) approach rather than forbidden (blacklist) ensures
/// that new fields added to <see cref="StepFields"/> are rejected by default,
/// forcing a conscious opt-in decision for each builder.
/// </remarks>
internal abstract class StepNodeBuilderBase : IStepNodeBuilder
{
    /// <summary>
    /// Gets the YAML step type name used in validation error messages (e.g. <c>"template"</c>, <c>"script"</c>).
    /// </summary>
    protected abstract string StepType { get; }

    /// <inheritdoc />
    public abstract bool CanBuild(StepFields fields);

    /// <inheritdoc />
    public StepNode Build(StepFields fields, SourceSpan span)
    {
        var violatingFields = GetViolatingFields(fields);

        return violatingFields.Count is > 0
            ? throw new YamlParseException(
                $"{StepType} step cannot contain fields: {string.Join(", ", violatingFields)}",
                span
            )
            : Create(fields, span);
    }

    /// <summary>
    /// Creates the concrete <see cref="StepNode"/> after all field validation has passed.
    /// </summary>
    /// <param name="fields">The parsed step fields.</param>
    /// <param name="span">The source location of the step.</param>
    /// <returns>The constructed <see cref="StepNode"/>.</returns>
    protected abstract StepNode Create(StepFields fields, SourceSpan span);

    /// <summary>
    /// Returns the set of YAML field names this builder explicitly allows.
    /// Any populated <see cref="StepFields"/> property whose name is not in this set
    /// will be reported as a violation and cause <see cref="Build"/> to throw.
    /// </summary>
    /// <remarks>
    /// Subclasses must include every field name they intentionally consume.
    /// </remarks>
    /// <returns>A set of allowed lowercase YAML field names.</returns>
    protected abstract IReadOnlySet<string> GetAllowedFields();

    /// <summary>
    /// Checks each populated field in <paramref name="fields"/> against the allowed set
    /// and returns the names of any that are not permitted.
    /// <para>
    /// Field names are hardcoded here (no reflection) for rename-safety and zero overhead.
    /// When a new property is added to <see cref="StepFields"/>, it must be registered below.
    /// </para>
    /// </summary>
    private List<string> GetViolatingFields(StepFields fields)
    {
        var allowed = GetAllowedFields();
        var violations = new List<string>();

        if (fields.Script is not null && !allowed.Contains("script"))
        {
            violations.Add("script");
        }

        if (fields.Template is not null && !allowed.Contains("template"))
        {
            violations.Add("template");
        }

        if (fields.DisplayName is not null && !allowed.Contains("displayName"))
        {
            violations.Add("displayName");
        }

        if (fields.Condition is not null && !allowed.Contains("condition"))
        {
            violations.Add("condition");
        }

        if (fields.TimeoutInMinutes is not null && !allowed.Contains("timeoutInMinutes"))
        {
            violations.Add("timeoutInMinutes");
        }

        if (fields.WorkingDirectory is not null && !allowed.Contains("workingDirectory"))
        {
            violations.Add("workingDirectory");
        }

        if (fields.Env.Count is > 0 && !allowed.Contains("env"))
        {
            violations.Add("env");
        }

        if (fields.Parameters.Count is > 0 && !allowed.Contains("parameters"))
        {
            violations.Add("parameters");
        }

        return violations;
    }
}
