using System.Reflection;

namespace GitHooks.Diagnostics.Tests;

public sealed class DiagnosticDescriptorsTests
{
    [Fact]
    public void AllCodes_AreUnique()
    {
        var descriptors =
            typeof(DiagnosticDescriptors)
                .GetFields(
                    BindingFlags.Public |
                    BindingFlags.Static)
                .Select(x => (DiagnosticDescriptor)x.GetValue(null)!)
                .ToArray();

        var duplicates =
            descriptors
                .GroupBy(x => x.Code)
                .Where(x => x.Count() > 1)
                .ToArray();

        Assert.Empty(duplicates);
    }
}
