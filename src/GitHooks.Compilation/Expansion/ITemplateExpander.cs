using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Mappings;

namespace GitHooks.Compilation.Expansion;

public interface ITemplateExpander
{
    public PipelineNode Expand(
        PipelineNode pipeline,
        ExpansionContext context,
        DiagnosticBag diagnostics);
}
