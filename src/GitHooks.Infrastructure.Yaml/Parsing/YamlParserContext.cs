using GitHooks.Compilation.Parsing;
using GitHooks.Diagnostics;

namespace GitHooks.Infrastructure.Yaml.Parsing;

internal sealed class YamlParserContext
{
    public YamlParserContext(
        ParsingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Cursor = YamlParserCursor.Create(
            context.Text,
            context.Document);

        Diagnostics = context.Diagnostics;
    }

    public YamlParserCursor Cursor { get; }

    public DiagnosticBag Diagnostics { get; }
}
