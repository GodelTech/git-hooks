using YamlDotNet.Core;

namespace GitHooks.Pipeline.Ast;

/// <summary>
/// Represents a single executable pipeline step.
/// </summary>
/// <param name="start">The source location where the step starts.</param>
public abstract record StepNode(Mark start)
    : AstNode(start);
