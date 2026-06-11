using GitHooks.Infrastructure.Yaml.Parsing;
using GitHooks.Infrastructure.Yaml.Tests.Testing;

using YamlDotNet.Core;
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
                    TestParserFactory.CreateDummyContext()));

        Assert.Equal(
            "key",
            exception.ParamName);
    }

    [Fact]
    public void MarkSeen_NullContext_Throws()
    {
        var tracker = new FieldTracker();

        var exception =
            Assert.Throws<ArgumentNullException>(
                () => tracker.MarkSeen(
                    new Scalar("test"),
                    null!));

        Assert.Equal(
            "context",
            exception.ParamName);
    }

    [Fact]
    public void MarkSeen_DuplicateField_Throws()
    {
        var tracker = new FieldTracker();

        var context =
            TestParserFactory.CreateContext(
                "test");

        context.Cursor.StartDocument();

        var key = new Scalar("test");

        tracker.MarkSeen(
            key,
            context);

        var exception =
            Assert.Throws<YamlException>(
                () => tracker.MarkSeen(
                    key,
                    context));

        Assert.StartsWith(
            "Duplicate 'test' field",
            exception.Message);
    }
}
