using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Common;

namespace GitHooks.Testing.Ast;

public static class TestFields
{
    public static StringKeyFieldNode<ExpressionNode> StringKey(
        string key,
        ExpressionNode value)
    {
        return new StringKeyFieldNode<ExpressionNode>
        {
            Key = key,
            Value = value,
            Span = SourceSpan.Unknown
        };
    }

    public static StringKeyFieldNode<ExpressionNode> StringKey(
        string key,
        string value = "test")
    {
        return StringKey(
            key,
            TestExpressions.String(value));
    }

    public static ComplexKeyFieldNode<AstNode> ComplexKey(
        string key,
        AstNode value)
    {
        return new ComplexKeyFieldNode<AstNode>
        {
            Key = TestValues.Scalar(key),
            Value = value,
            Span = SourceSpan.Unknown
        };
    }

    public static MappingFieldNode<StringKeyFieldNode<ExpressionNode>> Mapping(
        string key,
        StringKeyFieldNode<ExpressionNode> field)
    {
        return new MappingFieldNode<StringKeyFieldNode<ExpressionNode>>
        {
            Key = key,
            Fields = [field],
            Span = SourceSpan.Unknown
        };
    }
}
