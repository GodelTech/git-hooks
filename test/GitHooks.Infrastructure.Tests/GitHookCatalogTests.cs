using GitHooks.Infrastructure.Git;
using GitHooks.Infrastructure.Hooks;

namespace GitHooks.Infrastructure.Tests;

public sealed class GitHookCatalogTests
{
    [Fact]
    public void Catalog_ContainsElevenHooksAcrossFiveCategories()
    {
        var catalog = GitHookCatalog.Default;

        Assert.Equal(11, catalog.GetAllHooks().Count);
        Assert.Equal(4, catalog.GetCommitHooks().Count);
        Assert.Single(catalog.GetPushHooks());
        Assert.Equal(2, catalog.GetMergeHooks().Count);
        Assert.Equal(2, catalog.GetRebaseHooks().Count);
        Assert.Equal(2, catalog.GetCheckoutHooks().Count);
        Assert.Empty(catalog.GetHooks((GitHookCategory)999));
    }

    [Fact]
    public void ResolveHooks_NormalizesNamesAndSeparatesUnknownHooks()
    {
        var result = GitHookCatalog.Default.ResolveHooks([" PRE-COMMIT ", "pre-commit", "unknown", " "]);

        Assert.False(result.IsValid);
        Assert.Equal("pre-commit", Assert.Single(result.SupportedHooks).Name);
        Assert.Equal("unknown", Assert.Single(result.InvalidHooks));
        Assert.True(GitHookCatalog.Default.ResolveHooks(["pre-push"]).IsValid);
    }

    [Theory]
    [InlineData(GitConfigScope.Local, "local")]
    [InlineData(GitConfigScope.Global, "global")]
    [InlineData(GitConfigScope.System, "system")]
    public void ToGitString_MapsSupportedScope(GitConfigScope scope, string expected)
    {
        Assert.Equal(expected, scope.ToGitString());
    }

    [Fact]
    public void ToGitString_InvalidScope_Throws()
    {
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => ((GitConfigScope)999).ToGitString());
    }
}
