using GitHooks.Domain.Common;
using GitHooks.Testing.Common;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Tests.Testing;

internal static class TestScalar
{
    public static Scalar Create(
        string value)
    {
        return Create(
            value,
            TestSourceSpan.Unknown);
    }

    public static Scalar Create(
        string value,
        SourceSpan span)
    {
        return new Scalar(
            AnchorName.Empty,
            TagName.Empty,
            value,
            ScalarStyle.Any,
            true,
            true,
            new Mark(0, span.Start.Line, span.Start.Column),
            new Mark(0, span.End.Line, span.End.Column));
    }
}
