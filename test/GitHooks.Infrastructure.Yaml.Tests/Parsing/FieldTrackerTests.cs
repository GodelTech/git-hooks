using GitHooks.Infrastructure.Yaml.Parsing;
using GitHooks.Infrastructure.Yaml.Parsing.Exceptions;
using GitHooks.Infrastructure.Yaml.Tests.Testing;

using YamlDotNet.Core.Events;

namespace GitHooks.Infrastructure.Yaml.Tests.Parsing;

public sealed class FieldTrackerTests
{
    [Fact]
    public void MarkSeen_NullKey_Throws()
    {
        var tracker = new FieldTracker();

        var exception =
            Assert.Throws<ArgumentNullException>(
                () => tracker.MarkSeen(
                    null!,
                    TestParserFactory.CreateDummyCursor()));

        Assert.Equal(
            "key",
            exception.ParamName);
    }

    [Fact]
    public void MarkSeen_NullCursor_Throws()
    {
        var tracker = new FieldTracker();

        var exception =
            Assert.Throws<ArgumentNullException>(
                () => tracker.MarkSeen(
                    new Scalar("test"),
                    null!));

        Assert.Equal(
            "cursor",
            exception.ParamName);
    }

    [Fact]
    public void MarkSeen_DuplicateField_Throws()
    {
        var tracker = new FieldTracker();

        var cursor =
            TestParserFactory.CreateCursor(
                "test");

        cursor.StartDocument();

        var key = new Scalar("test");

        tracker.MarkSeen(
            key,
            cursor);

        var exception =
            Assert.Throws<YamlPipelineParsingException>(
                () => tracker.MarkSeen(
                    key,
                    cursor));

        Assert.StartsWith(
            "Duplicate 'test' field",
            exception.Message);
    }
}
