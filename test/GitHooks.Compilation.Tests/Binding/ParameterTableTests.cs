using GitHooks.Compilation.Binding;
using GitHooks.Testing.Ast.Builders.Mappings.Parameters;

namespace GitHooks.Compilation.Tests.Binding;

public sealed class ParameterTableTests
{
    [Fact]
    public void Parameters_NewInstance_IsEmpty()
    {
        var table = new ParameterTable();

        Assert.Empty(table.Parameters);
    }

    [Fact]
    public void AddParameter_NullParameter_Throws()
    {
        var table = new ParameterTable();

        Assert.Throws<ArgumentNullException>(
            () => table.AddParameter(null!));
    }

    [Fact]
    public void AddParameter_ValidParameter_IsAddedAndRetrievable()
    {
        var table = new ParameterTable();

        var parameter = new ParameterNodeBuilder()
            .WithName("serverName")
            .Build();

        table.AddParameter(parameter);

        Assert.True(table.ContainsParameter("serverName"));

        Assert.True(
            table.TryGetParameter(
                "serverName",
                out var found));

        Assert.Same(
            parameter,
            found);
    }

    [Fact]
    public void ContainsParameter_IsCaseInsensitive()
    {
        var table = new ParameterTable();

        var parameter = new ParameterNodeBuilder()
            .WithName("serverName")
            .Build();

        table.AddParameter(parameter);

        Assert.True(table.ContainsParameter("SERVERNAME"));
    }

    [Fact]
    public void AddParameter_BlankName_IsNotAdded()
    {
        var table = new ParameterTable();

        var parameter = new ParameterNodeBuilder()
            .WithName("   ")
            .Build();

        table.AddParameter(parameter);

        Assert.Empty(table.Parameters);
    }

    [Fact]
    public void AddParameter_DuplicateName_KeepsFirstDeclaration()
    {
        var table = new ParameterTable();

        var first = new ParameterNodeBuilder()
            .WithName("serverName")
            .Build();

        var second = new ParameterNodeBuilder()
            .WithName("serverName")
            .Build();

        table.AddParameter(first);
        table.AddParameter(second);

        Assert.Single(table.Parameters);

        Assert.True(
            table.TryGetParameter(
                "serverName",
                out var found));

        Assert.Same(
            first,
            found);
    }

    [Fact]
    public void TryGetParameter_UnknownName_ReturnsFalse()
    {
        var table = new ParameterTable();

        Assert.False(
            table.TryGetParameter(
                "unknown",
                out var found));

        Assert.Null(found);
    }
}
