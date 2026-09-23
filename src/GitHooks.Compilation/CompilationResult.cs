using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Mappings;

namespace GitHooks.Compilation;

public sealed class CompilationResult(
    PipelineNode root,
    PipelineNode boundRoot,
    DiagnosticBag diagnostics)
{
    public PipelineNode Root { get; }
        = root ?? throw new ArgumentNullException(nameof(root));

    public PipelineNode BoundRoot { get; }
        = boundRoot ?? throw new ArgumentNullException(nameof(boundRoot));

    public DiagnosticBag Diagnostics { get; }
        = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
}
