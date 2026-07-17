using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Mappings;

namespace GitHooks.Compilation;

public sealed class CompilationResult(
    PipelineNode root,
    DiagnosticBag diagnostics)
{
    public PipelineNode Root { get; }
        = root ?? throw new ArgumentNullException(nameof(root));

    public DiagnosticBag Diagnostics { get; }
        = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
}
