using System.Collections;

using GitHooks.Domain.Ast;

namespace GitHooks.Validation.Rules;

internal sealed class RuleCollection<TNode>
    : IReadOnlyCollection<IValidationRule<TNode>>, IRuleCollection
    where TNode : AstNode
{
    private readonly List<IValidationRule<TNode>> _rules
        = [];

    public int Count
        => _rules.Count;

    public void Add(
        IValidationRule<TNode> rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        _rules.Add(rule);
    }

    public IEnumerator<IValidationRule<TNode>> GetEnumerator()
    {
        return _rules.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
