using GitHooks.Domain.Ast;

namespace GitHooks.Validation.Rules;

internal interface IValidationRule<in TNode>
    where TNode : AstNode
{
    public void Validate(
        TNode node,
        ValidationContext context);
}
