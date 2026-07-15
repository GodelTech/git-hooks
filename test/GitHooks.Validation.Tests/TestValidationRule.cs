using GitHooks.Domain.Ast;
using GitHooks.Validation.Rules;

namespace GitHooks.Validation.Tests;

internal sealed class TestValidationRule<TNode>
    : IValidationRule<TNode>
    where TNode : AstNode
{
    public void Validate(
        TNode node,
        ValidationContext context)
    {
    }
}
