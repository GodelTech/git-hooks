using GitHooks.Domain.Ast.Mappings;

namespace GitHooks.Compilation;

public interface IPipelineAstCompiler
{
    public PipelineNode Compile(
        SourceContent source,
        CompilationContext context);
}
