using GitHooks.Domain.Ast.Unknown;
using GitHooks.Domain.Common;

namespace GitHooks.Testing.Ast;

public static class TestUnknown
{
    public static UnknownSimpleFieldNode SimpleField(
        string key,
        UnknownNode value)
    {
        return new()
        {
            Key = key,
            Value = value,
            Span = SourceSpan.Unknown
        };
    }

    public static UnknownSimpleFieldNode SimpleField(
        string key,
        string value)
    {
        return SimpleField(
            key,
            Scalar(value));
    }

    public static UnknownComplexFieldNode ComplexField(
        string key,
        UnknownNode value)
    {
        return new()
        {
            Key = Scalar(key),
            Value = value,
            Span = SourceSpan.Unknown
        };
    }

    public static UnknownSequenceNode Sequence(
        IEnumerable<UnknownNode> items)
    {
        return new()
        {
            Items = [.. items],
            Span = SourceSpan.Unknown
        };
    }

    public static UnknownMappingNode Mapping(
        IEnumerable<UnknownFieldNode> fields)
    {
        return new()
        {
            Fields = [.. fields],
            Span = SourceSpan.Unknown
        };
    }

    public static UnknownScalarNode Scalar(
        string value)
    {
        return new()
        {
            Value = value,
            Span = SourceSpan.Unknown
        };
    }
}
