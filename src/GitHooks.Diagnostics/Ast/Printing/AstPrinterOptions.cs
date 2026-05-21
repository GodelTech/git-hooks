namespace GitHooks.Diagnostics.Ast.Printing;

public sealed record AstPrinterOptions
{
    public static AstPrinterOptions Default { get; }
        = new();

    public bool IncludeNodeKinds { get; init; }

    public bool IncludeSourceSpans { get; init; }

    public int IndentSize { get; init; } = 2;
}
