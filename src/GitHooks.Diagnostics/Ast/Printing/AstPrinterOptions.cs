namespace GitHooks.Diagnostics.Ast.Printing;

public sealed class AstPrinterOptions
{
    public static AstPrinterOptions Default { get; }
        = new();

    // todo: printer must show NodeKind for each node, so that we can verify it in the snapshot
    public bool IncludeNodeKinds { get; init; }

    // todo: printer must show SourceSpan for each node, so that we can verify it in the snapshot
    public bool IncludeSourceSpans { get; init; }

    public int IndentSize { get; init; } = 2;
}
