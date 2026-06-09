using GitHooks.Diagnostics;
using GitHooks.Domain.Ast;

namespace GitHooks.Validation;

public interface IValidationRule<in TNode>
    where TNode : AstNode
{
    public void Validate(TNode node, DiagnosticBag diagnostics);
}
