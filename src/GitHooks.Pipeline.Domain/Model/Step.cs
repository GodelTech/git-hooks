using System.Diagnostics.CodeAnalysis;

namespace GitHooks.Pipeline.Domain.Model;

/// <summary>
/// Represents a single pipeline step.
/// </summary>
/// <param name="Id">The stable step identifier.</param>
/// <param name="Location">The source location where this step starts.</param>
[SuppressMessage(
    "Naming",
    "CA1716:Identifiers should not match keywords",
    Justification = "Step is an intentional ubiquitous domain term in the public model.")]
public abstract record Step(StepId Id, SourceLocation Location);
