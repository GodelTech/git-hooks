using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Mappings;

namespace GitHooks.Compilation.Expansion;

internal sealed class TemplateExpander : ITemplateExpander
{
    public PipelineNode Expand(
        PipelineNode pipeline,
        ExpansionContext context,
        DiagnosticBag diagnostics)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(diagnostics);

        return pipeline;
    }
}
