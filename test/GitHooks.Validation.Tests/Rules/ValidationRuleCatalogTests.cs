using System.Collections;
using System.Reflection;

using GitHooks.Validation.Rules;

namespace GitHooks.Validation.Tests.Rules;

public sealed class ValidationRuleCatalogTests
{
    [Fact]
    public void RegisterBuiltInRules_NullRegistry_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => ValidationRuleCatalog.RegisterBuiltInRules(
                null!));

        Assert.Equal(
            "registry",
            exception.ParamName);
    }

    // Ensures that every built-in validation rule is registered in
    // ValidationRuleCatalog. This prevents new rules from being
    // accidentally implemented without being added to the catalog.
    [Fact]
    public void ValidationRuleCatalog_ContainsAllValidationRules()
    {
        var expected = typeof(IValidationRule<>).Assembly
            .GetTypes()
            .Where(static type =>
                type is
                {
                    IsAbstract: false,
                    IsInterface: false,
                    IsGenericTypeDefinition: false,
                    Namespace: not null
                }

                &&
                type.Namespace.StartsWith(
                    "GitHooks.Validation.Rules",
                    StringComparison.Ordinal))
            .Where(static type =>
                type.GetInterfaces()
                    .Any(static @interface =>
                        @interface.IsGenericType &&
                        @interface.GetGenericTypeDefinition() == typeof(IValidationRule<>)))
            .OrderBy(static type => type.FullName)
            .ToArray();

        var actual = typeof(ValidationRuleCatalog)
            .GetProperties(
                BindingFlags.Public | BindingFlags.Static)
            .Where(static property =>
                property.PropertyType.IsGenericType &&
                property.PropertyType.GetGenericTypeDefinition() == typeof(RuleCollection<>))
            .SelectMany(static property =>
                ((IEnumerable)property.GetValue(null)!)
                    .Cast<object>())
            .Select(static rule => rule.GetType())
            .OrderBy(static type => type.FullName)
            .ToArray();

        var missing = expected
            .Except(actual)
            .ToArray();

        var unexpected = actual
            .Except(expected)
            .ToArray();

        var missingRules = missing.Length == 0
            ? "  <none>"
            : string.Join(
                Environment.NewLine,
                missing.Select(static x => $"  - {x.FullName}"));

        var unexpectedRules = unexpected.Length == 0
            ? "  <none>"
            : string.Join(
                Environment.NewLine,
                unexpected.Select(static x => $"  - {x.FullName}"));

        var message =
            $"""
            ValidationRuleCatalog is out of sync.

            Missing rules:
            {missingRules}

            Unexpected rules:
            {unexpectedRules}
            """;

        Assert.True(
            missing.Length == 0 && unexpected.Length == 0,
            message);
    }
}
