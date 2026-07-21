using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Mappings;

namespace GitHooks.Compilation.Validation;

public interface IPipelineValidator
{
    public void Validate(
        PipelineNode root,
        DiagnosticBag diagnostics);
}
