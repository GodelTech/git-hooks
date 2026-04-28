using YamlDotNet.Core;

namespace GitHooks.Pipeline.Ast;

public abstract record AstNode(Mark Start)
{
    /// <summary>
    /// Gets the 1-based line in source YAML where this node begins.
    /// </summary>
    public long Line => Start.Line;

    /// <summary>
    /// Gets the 1-based column in source YAML where this node begins.
    /// </summary>
    public long Column => Start.Column;
}
