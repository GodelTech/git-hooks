using GitHooks.Domain.Ast.Mappings.Steps;

namespace GitHooks.Testing.Ast.Builders.Mappings.Steps;

public abstract class StepNodeBuilder<TBuilder, TNode>
    : PipelineNodeBuilderBase<TBuilder, TNode>
    where TBuilder : StepNodeBuilder<TBuilder, TNode>
    where TNode : StepNode;
