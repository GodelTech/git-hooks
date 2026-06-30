using GitHooks.Domain.Ast;

namespace GitHooks.Validation.Rules;

internal sealed class ValidationRuleRegistry
{
    private readonly Dictionary<Type, IRuleCollection> _collections
        = [];

    public ValidationRuleRegistry()
    {
        this.RegisterBuiltInRules();
    }

    public IReadOnlyCollection<IValidationRule<TNode>> GetRules<TNode>()
        where TNode : AstNode
    {
        if (_collections.TryGetValue(typeof(TNode), out var collection))
        {
            return (RuleCollection<TNode>)collection;
        }

        return [];
    }

    internal void Register<TNode>(
        IEnumerable<IValidationRule<TNode>> rules)
        where TNode : AstNode
    {
        ArgumentNullException.ThrowIfNull(rules);

        foreach (var rule in rules)
        {
            Register(rule);
        }
    }

    internal void Register<TNode>(
        IValidationRule<TNode> rule)
        where TNode : AstNode
    {
        ArgumentNullException.ThrowIfNull(rule);

        GetOrCreateCollection<TNode>()
            .Add(rule);
    }

    private RuleCollection<TNode> GetOrCreateCollection<TNode>()
        where TNode : AstNode
    {
        if (_collections.TryGetValue(typeof(TNode), out var collection))
        {
            return (RuleCollection<TNode>)collection;
        }

        var created = new RuleCollection<TNode>();

        _collections.Add(
            typeof(TNode),
            created);

        return created;
    }
}
