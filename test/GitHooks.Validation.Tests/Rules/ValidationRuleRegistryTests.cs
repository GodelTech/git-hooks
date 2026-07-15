using System.Collections;
using System.Reflection;

using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Visitors;
using GitHooks.Validation.Rules;

namespace GitHooks.Validation.Tests.Rules;

public sealed class ValidationRuleRegistryTests
{
    // Ensures that every rule collection defined in ValidationRuleCatalog
    // is registered by ValidationRuleRegistry. This test automatically
    // detects newly added rule collections that are not registered.
    [Fact]
    public void Constructor_RegistersAllRuleCollectionsFromCatalog()
    {
        var registry = new ValidationRuleRegistry();

        var getRulesMethod = typeof(ValidationRuleRegistry)
            .GetMethod(
                nameof(ValidationRuleRegistry.GetRules))!;

        var properties = typeof(ValidationRuleCatalog)
            .GetProperties(
                BindingFlags.Public | BindingFlags.Static)
            .Where(static property =>
                property.PropertyType.IsGenericType &&
                property.PropertyType.GetGenericTypeDefinition() == typeof(RuleCollection<>));

        foreach (var property in properties)
        {
            var expected =
                (IEnumerable)property.GetValue(null)!;

            var nodeType = property.PropertyType
                .GetGenericArguments()[0];

            var actual =
                (IEnumerable)getRulesMethod
                    .MakeGenericMethod(nodeType)
                    .Invoke(registry, null)!;

            Assert.True(
                expected.Cast<object>().SequenceEqual(actual.Cast<object>()),
                $"Rule collection '{property.Name}' is not registered correctly.");
        }
    }

    [Fact]
    public void GetRules_UnregisteredNode_ReturnsEmptyCollection()
    {
        var registry = new ValidationRuleRegistry();

        var rules = registry.GetRules<TestNode>();

        Assert.Empty(rules);
    }

    [Fact]
    public void Register_NullRule_Throws()
    {
        var registry = new ValidationRuleRegistry();

        var exception =
            Assert.Throws<ArgumentNullException>(
                () => registry.Register(
                    (IValidationRule<TestNode>)null!));

        Assert.Equal(
            "rule",
            exception.ParamName);
    }

    [Fact]
    public void Register_Rule_RegistersRule()
    {
        var registry = new ValidationRuleRegistry();

        var rule = new TestValidationRule<TestNode>();

        registry.Register(rule);

        Assert.Contains(
            rule,
            registry.GetRules<TestNode>());
    }

    [Fact]
    public void Register_NullRules_Throws()
    {
        var registry = new ValidationRuleRegistry();

        var exception =
            Assert.Throws<ArgumentNullException>(
                () => registry.Register(
                    (IEnumerable<IValidationRule<TestNode>>)null!));

        Assert.Equal(
            "rules",
            exception.ParamName);
    }

    [Fact]
    public void Register_Rules_RegistersRules()
    {
        var registry = new ValidationRuleRegistry();

        var rule1 = new TestValidationRule<TestNode>();
        var rule2 = new TestValidationRule<TestNode>();

        registry.Register(
        [
            rule1,
            rule2
        ]);

        Assert.Collection(
            registry.GetRules<TestNode>(),
            rule => Assert.Same(rule1, rule),
            rule => Assert.Same(rule2, rule));
    }

    private sealed class TestNode
        : AstNode
    {
        public override AstNodeKind Kind => default;

        public override void Accept(IAstCommandVisitor visitor)
        {
            throw new NotSupportedException();
        }

        public override TResult Accept<TResult>(IAstQueryVisitor<TResult> visitor)
        {
            throw new NotSupportedException();
        }
    }
}
