using GitHooks.Diagnostics;
using GitHooks.Domain.Ast.Mappings;

namespace GitHooks.Compilation.Parsing;

public interface IPipelineParser
{
    public PipelineNode Parse(
        ParsingContext context,
        DiagnosticBag diagnostics);
}
