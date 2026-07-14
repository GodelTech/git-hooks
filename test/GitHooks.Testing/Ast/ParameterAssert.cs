using System.Diagnostics;

using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Syntax;

namespace GitHooks.Testing.Ast;

public static class ParameterAssert
{
    public static StringKeyFieldNode<ExpressionNode> GetRequiredField(
        ParameterNode parameter,
        string fieldName)
    {
        ArgumentNullException.ThrowIfNull(parameter);
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);

        return fieldName switch
        {
            ParameterFieldNames.Name
                => parameter.Name,

            ParameterFieldNames.DisplayName
                => parameter.DisplayName
                    ?? AssertMissingField(fieldName),

            ParameterFieldNames.Type
                => parameter.Type
                    ?? AssertMissingField(fieldName),

            ParameterFieldNames.DefaultValue
                => parameter.DefaultValue
                    ?? AssertMissingField(fieldName),

            _ => throw new ArgumentOutOfRangeException(
                nameof(fieldName),
                fieldName,
                "Unknown parameter field."),
        };
    }

    private static StringKeyFieldNode<ExpressionNode> AssertMissingField(
        string fieldName)
    {
        Assert.Fail($"Expected parameter field '{fieldName}' to be present.");

        throw new UnreachableException();
    }
}
