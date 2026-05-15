using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Application.Binding;
using GitHooks.Workflow.Application.Binding.Parameters;
using GitHooks.Workflow.Application.Binding.Steps;
using GitHooks.Workflow.Application.Binding.UnknownNodes;
using GitHooks.Workflow.Domain.Model;

namespace GitHooks.Workflow.Application.Tests.Binding.Steps;

public class ScriptStepParameterBinderTests
{
    [Fact]
    public void Bind_ScriptWithParameterReference_ExpandsScript()
    {
        var step = CreateScriptStep("echo ${{ parameters.message }}");
        var context = CreateContext([("message", "Hello World")]);
        var binder = CreateBinder();

        var result = binder.Bind(step, context);

        Assert.Equal("echo Hello World", result.Script.Value);
    }

    [Fact]
    public void Bind_DisplayNameWithParameterReference_ExpandsDisplayName()
    {
        var step = CreateScriptStep("echo ok", displayName: "Deploy ${{ parameters.env }}");
        var context = CreateContext([("env", "Production")]);
        var binder = CreateBinder();

        var result = binder.Bind(step, context);

        Assert.Equal("Deploy Production", result.DisplayName);
    }

    [Fact]
    public void Bind_WorkingDirectoryWithParameterReference_ExpandsWorkingDirectory()
    {
        var step = CreateScriptStep(
            "echo ok",
            workingDirectory: "./build/${{ parameters.variant }}"
        );
        var context = CreateContext([("variant", "debug")]);
        var binder = CreateBinder();

        var result = binder.Bind(step, context);

        Assert.Equal("./build/debug", result.WorkingDirectory?.Value);
    }

    [Fact]
    public void Bind_EnvWithParameterReference_ExpandsEnv()
    {
        var step = CreateScriptStep(
            "echo ok",
            env: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["BUILD_CONFIG"] = "${{ parameters.buildConfig }}"
            }
        );
        var context = CreateContext([("buildConfig", "Release")]);
        var binder = CreateBinder();

        var result = binder.Bind(step, context);

        Assert.Equal("Release", result.Env["BUILD_CONFIG"].Value);
    }

    [Fact]
    public void Bind_MultipleEnvVarsWithParameterReferences_ExpandsAll()
    {
        var step = CreateScriptStep(
            "echo ok",
            env: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["SERVER"] = "${{ parameters.server }}",
                ["PORT"] = "${{ parameters.port }}"
            }
        );
        var context = CreateContext([("server", "localhost"), ("port", "8080")]);
        var binder = CreateBinder();

        var result = binder.Bind(step, context);

        Assert.Equal("localhost", result.Env["SERVER"].Value);
        Assert.Equal("8080", result.Env["PORT"].Value);
    }

    [Fact]
    public void Bind_AllFieldsWithParameterReferences_ExpandsAll()
    {
        var step = CreateScriptStep(
            "echo ${{ parameters.message }}",
            displayName: "Task ${{ parameters.taskName }}",
            workingDirectory: "./${{ parameters.folder }}",
            env: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["MESSAGE"] = "${{ parameters.message }}"
            }
        );
        var context = CreateContext([
            ("message", "Success"),
            ("taskName", "Deploy"),
            ("folder", "dist")
        ]);
        var binder = CreateBinder();

        var result = binder.Bind(step, context);

        Assert.Equal("echo Success", result.Script.Value);
        Assert.Equal("Task Deploy", result.DisplayName);
        Assert.Equal("./dist", result.WorkingDirectory?.Value);
        Assert.Equal("Success", result.Env["MESSAGE"].Value);
    }

    [Fact]
    public void Bind_NoParameterReferences_LeaveFieldsUnchanged()
    {
        var step = CreateScriptStep(
            "echo hello",
            displayName: "Simple task",
            workingDirectory: "./src"
        );
        var context = CreateContext([]);
        var binder = CreateBinder();

        var result = binder.Bind(step, context);

        Assert.Equal("echo hello", result.Script.Value);
        Assert.Equal("Simple task", result.DisplayName);
        Assert.Equal("./src", result.WorkingDirectory?.Value);
    }

    [Fact]
    public void Bind_NullDisplayName_PreservesNull()
    {
        var step = CreateScriptStep("echo ok", displayName: null);
        var context = CreateContext([]);
        var binder = CreateBinder();

        var result = binder.Bind(step, context);

        Assert.Null(result.DisplayName);
    }

    [Fact]
    public void Bind_NullWorkingDirectory_PreservesNull()
    {
        var step = CreateScriptStep("echo ok", workingDirectory: null);
        var context = CreateContext([]);
        var binder = CreateBinder();

        var result = binder.Bind(step, context);

        Assert.Null(result.WorkingDirectory);
    }

    [Fact]
    public void Bind_UnknownFieldsWithParameterReferences_ExpandsValues()
    {
        var unknownSpan = CreateSpan();
        var inputsField = new UnknownFieldNode(
            new UnknownScalarNode("inputs", unknownSpan),
            new UnknownMappingNode(
                [
                    new UnknownMappingEntryNode(
                        new UnknownScalarNode("target", unknownSpan),
                        new UnknownScalarNode("${{ parameters.target }}", unknownSpan),
                        unknownSpan
                    )
                ],
                unknownSpan
            ),
            unknownSpan
        );

        var step = CreateScriptStep("echo ok", unknownFields: [inputsField]);
        var context = CreateContext([("target", "production")]);
        var binder = CreateBinder();

        var result = binder.Bind(step, context);

        var field = Assert.Single(result.UnknownFields);
        var mapping = Assert.IsType<UnknownMappingNode>(field.Value);
        var entry = Assert.Single(mapping.Entries);
        var value = Assert.IsType<UnknownScalarNode>(entry.Value);

        Assert.Equal("production", value.Value);
    }

    private static ScriptStepParameterBinder CreateBinder()
    {
        var unknownNodeBinder = new UnknownNodeParameterBinder();
        return new ScriptStepParameterBinder(unknownNodeBinder);
    }

    private static ScriptStepNode CreateScriptStep(
        string script,
        string? displayName = null,
        string? workingDirectory = null,
        IReadOnlyDictionary<string, string>? env = null,
        IReadOnlyList<UnknownFieldNode>? unknownFields = null,
        SourceSpan? span = null)
    {
        var effectiveSpan = span ?? CreateSpan();
        var boundEnv = env?.ToDictionary(
            pair => pair.Key,
            pair => new InterpolatedStringNode(pair.Value),
            StringComparer.Ordinal
        ) ?? new Dictionary<string, InterpolatedStringNode>(StringComparer.Ordinal);

        return new ScriptStepNode(
            new InterpolatedStringNode(script),
            unknownFields ?? [],
            effectiveSpan
        )
        {
            DisplayName = displayName,
            WorkingDirectory = workingDirectory is null
                ? null
                : new InterpolatedStringNode(workingDirectory),
            Env = boundEnv
        };
    }

    private static ParameterBindingContext CreateContext(
        IEnumerable<(string Name, string Value)> parameters)
    {
        var declarations = new Dictionary<string, ParameterNode>(StringComparer.Ordinal);
        var parameterValues = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var (name, value) in parameters)
        {
            parameterValues[name] = value;
        }

        return new ParameterBindingContext(declarations, parameterValues);
    }

    private static SourceSpan CreateSpan(string sourceName = "test")
    {
        return SourceSpan.Unknown(new SourceRef(sourceName));
    }
}
