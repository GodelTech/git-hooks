using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Validation.Rules;

namespace GitHooks.Validation.Tests.Rules;

public sealed class RuleCollectionTests
{
    [Fact]
    public void Add_NullRule_Throws()
    {
        var rules = new RuleCollection<ParameterNode>();

        var exception =
            Assert.Throws<ArgumentNullException>(
                () => rules.Add(null!));

        Assert.Equal(
            "rule",
            exception.ParamName);
    }

    [Fact]
    public void Add_Rule_IncreasesCount()
    {
#pragma warning disable IDE0028 // Simplify collection initialization. Using a collection initializer would bypass the method under test.
        var rules = new RuleCollection<ParameterNode>();

        rules.Add(new TestValidationRule<ParameterNode>());
#pragma warning restore IDE0028 // Simplify collection initialization

        Assert.Single(rules);
    }

    [Fact]
    public void GetEnumerator_EmptyCollection_ReturnsNoItems()
    {
        var rules = new RuleCollection<ParameterNode>();

        Assert.Empty(rules);
    }

    [Fact]
    public void GetEnumerator_NonEmptyCollection_ReturnsRules()
    {
        var rules = new RuleCollection<ParameterNode>();

        var rule1 = new TestValidationRule<ParameterNode>();
        var rule2 = new TestValidationRule<ParameterNode>();

        rules.Add(rule1);
        rules.Add(rule2);

        Assert.Collection(
            rules,
            rule => Assert.Same(rule1, rule),
            rule => Assert.Same(rule2, rule));
    }
}
