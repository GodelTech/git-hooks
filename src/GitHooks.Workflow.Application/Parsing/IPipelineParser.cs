using GitHooks.Workflow.Application.Ast;

namespace GitHooks.Workflow.Application.Parsing;

public interface IPipelineParser
{
    public PipelineNode Parse(string content, string sourceName);
}
