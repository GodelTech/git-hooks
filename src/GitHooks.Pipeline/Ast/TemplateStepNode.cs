using YamlDotNet.Core;

namespace GitHooks.Pipeline.Ast;

/// <summary>
/// Represents a template include step.
/// </summary>
public sealed record TemplateStepNode : StepNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateStepNode"/> class.
    /// </summary>
    /// <param name="start">The source location where the step starts.</param>
    /// <param name="templatePath">The relative template file path.</param>
    /// <param name="parameters">Template parameters passed to the included template.</param>
    public TemplateStepNode(
        Mark start,
        string templatePath,
        IReadOnlyDictionary<string, string> parameters)
        : base(start)
    {
        TemplatePath = templatePath;
        Parameters = parameters;
    }

    /// <summary>
    /// Gets the relative template file path.
    /// </summary>
    public string TemplatePath { get; }

    /// <summary>
    /// Gets template parameters for expansion.
    /// </summary>
    public IReadOnlyDictionary<string, string> Parameters { get; }
}
