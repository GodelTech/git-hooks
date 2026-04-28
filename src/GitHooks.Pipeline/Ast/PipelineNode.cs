using YamlDotNet.Core;

namespace GitHooks.Pipeline.Ast;

/// <summary>
/// Represents the parsed pipeline root.
/// </summary>
public sealed record PipelineNode : AstNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineNode"/> class.
    /// </summary>
    /// <param name="start">The source location where the pipeline root starts.</param>
    /// <param name="steps">The ordered list of pipeline steps.</param>
    public PipelineNode(Mark start, IReadOnlyList<StepNode> steps)
        : base(start)
    {
        Steps = steps;
    }

    /// <summary>
    /// Gets the ordered list of steps that form the pipeline.
    /// </summary>
    public IReadOnlyList<StepNode> Steps { get; }
}
