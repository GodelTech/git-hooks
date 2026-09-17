using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Mappings;

namespace GitHooks.Compilation.Expansion;

public interface ITemplateCompiler
{
    public PipelineNode Compile(
        SourceContent source,
        DiagnosticBag diagnostics);
}
