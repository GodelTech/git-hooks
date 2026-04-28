using YamlDotNet.Core;

namespace GitHooks.Pipeline.Ast;

/// <summary>
/// Represents an inline script step.
/// </summary>
public sealed record ScriptStepNode : StepNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptStepNode"/> class.
    /// </summary>
    /// <param name="start">The source location where the step starts.</param>
    /// <param name="script">The script content to execute.</param>
    public ScriptStepNode(Mark start, string script)
        : base(start)
    {
        Script = script;
    }

    /// <summary>
    /// Gets the script content to execute.
    /// </summary>
    public string Script { get; }
}
