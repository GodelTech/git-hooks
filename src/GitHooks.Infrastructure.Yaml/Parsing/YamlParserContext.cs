using GitHooks.Compilation.Parsing;
using GitHooks.Diagnostics;

namespace GitHooks.Infrastructure.Yaml.Parsing;

internal sealed class YamlParserContext
{
    public YamlParserContext(
        ParsingContext context,
        DiagnosticBag diagnostics)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(diagnostics);

        Cursor = YamlParserCursor.Create(
            context.Text,
            context.Document);

        Diagnostics = diagnostics;
    }

    public YamlParserCursor Cursor { get; }

    public DiagnosticBag Diagnostics { get; }
}
