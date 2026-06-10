using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Common;

namespace GitHooks.Testing.Ast.Builders;

public sealed class ScriptStepNodeBuilder
    : MappingNodeBuilder<ScriptStepNodeBuilder>
{
    private readonly Dictionary<string, ExpressionNode> _env
        = new(StringComparer.OrdinalIgnoreCase);

    private ExpressionNode? _script;
    private ExpressionNode? _displayName;
    private ExpressionNode? _condition;
    private ExpressionNode? _timeoutInMinutes;
    private ExpressionNode? _workingDirectory;

    public ScriptStepNodeBuilder()
    {
        _script = new StringLiteralExpressionNode
        {
            Value = "dotnet test",
            Span = SourceSpan.Unknown
        };
    }

    public ScriptStepNodeBuilder WithScript(
        ExpressionNode script)
    {
        ArgumentNullException.ThrowIfNull(script);

        _script = script;

        return this;
    }

    public ScriptStepNodeBuilder WithScript(
        string script = "dotnet test")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(script);

        WithScript(
            TestExpressions.String(script));

        return this;
    }

    public ScriptStepNodeBuilder WithDisplayName(
        ExpressionNode displayName)
    {
        ArgumentNullException.ThrowIfNull(displayName);

        _displayName = displayName;

        return this;
    }

    public ScriptStepNodeBuilder WithDisplayName(string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        WithDisplayName(
            TestExpressions.String(displayName));

        return this;
    }

    public ScriptStepNodeBuilder WithCondition(
        ExpressionNode condition)
    {
        ArgumentNullException.ThrowIfNull(condition);

        _condition = condition;

        return this;
    }

    public ScriptStepNodeBuilder WithCondition(string condition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(condition);

        WithCondition(
            TestExpressions.String(condition));

        return this;
    }

    public ScriptStepNodeBuilder WithTimeoutInMinutes(
        ExpressionNode timeoutInMinutes)
    {
        ArgumentNullException.ThrowIfNull(timeoutInMinutes);

        _timeoutInMinutes = timeoutInMinutes;

        return this;
    }

    public ScriptStepNodeBuilder WithTimeoutInMinutes(int timeoutInMinutes)
    {
        WithTimeoutInMinutes(
            TestExpressions.Integer(timeoutInMinutes));

        return this;
    }

    public ScriptStepNodeBuilder WithWorkingDirectory(
        ExpressionNode workingDirectory)
    {
        ArgumentNullException.ThrowIfNull(workingDirectory);

        _workingDirectory = workingDirectory;

        return this;
    }

    public ScriptStepNodeBuilder WithWorkingDirectory(string workingDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workingDirectory);

        WithWorkingDirectory(
            TestExpressions.String(workingDirectory));

        return this;
    }

    public ScriptStepNodeBuilder WithEnvironmentVariable(
        string name,
        ExpressionNode value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(value);

        _env.Add(name, value);

        return this;
    }

    public ScriptStepNodeBuilder WithEnvironmentVariable(
        string name,
        string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        WithEnvironmentVariable(
            name,
            TestExpressions.String(value));

        return this;
    }

    public ScriptStepNode Build()
    {
        if (_script is null)
        {
            throw new InvalidOperationException("Script is required to build a ScriptStepNode.");
        }

        return new ScriptStepNode
        {
            Script = _script,
            DisplayName = _displayName,
            Condition = _condition,
            TimeoutInMinutes = _timeoutInMinutes,
            WorkingDirectory = _workingDirectory,
            Env = new Dictionary<string, ExpressionNode>(_env),
            UnknownFields = [.. UnknownFields],
            Span = Span
        };
    }
}
