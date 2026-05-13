using GitHooks.Workflow.Application.Ast;
using GitHooks.Workflow.Application.Ast.Expressions;
using GitHooks.Workflow.Application.Ast.Unknown;
using GitHooks.Workflow.Application.Binding;
using GitHooks.Workflow.Application.Binding.Exceptions;
using GitHooks.Workflow.Application.DependencyInjection;
using GitHooks.Workflow.Domain.Model;

using Microsoft.Extensions.DependencyInjection;

namespace GitHooks.Workflow.Application.Tests.Binding;

public class PipelineParameterBinderTests
{
    [Fact]
    public void Bind_RootParameterReferencedInKnownFields_ExpandsValues()
    {
        var pipeline = CreatePipeline(
            [
                CreateParameter("serverName", ParameterType.Text, "TEST-SERVER")
            ],
            [
                CreateScriptStep(
                    "echo ${{ parameters.serverName }}",
                    displayName: "Deploy ${{ parameters.serverName }}",
                    workingDirectory: "./artifacts/${{ parameters.serverName }}",
                    env: new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["SERVER_NAME"] = "${{ parameters.serverName }}"
                    }
                )
            ]
        );

        var result = CreateBinder().Bind(pipeline);

        var step = Assert.IsType<ScriptStepNode>(Assert.Single(result.Steps));

        Assert.Equal("echo TEST-SERVER", step.Script.Value);
        Assert.Equal("Deploy TEST-SERVER", step.DisplayName);
        Assert.Equal("./artifacts/TEST-SERVER", step.WorkingDirectory?.Value);
        Assert.Equal("TEST-SERVER", step.Env["SERVER_NAME"].Value);
    }

    [Fact]
    public void Bind_RootParameterReferencedInUnknownFields_ExpandsValues()
    {
        var unknownSpan = CreateSpan();

        var inputsField = new UnknownFieldNode(
            new UnknownScalarNode("inputs", unknownSpan),
            new UnknownMappingNode(
                [
                    new UnknownMappingEntryNode(
                        new UnknownScalarNode("target", unknownSpan),
                        new UnknownScalarNode("${{ parameters.serverName }}", unknownSpan),
                        unknownSpan
                    )
                ],
                unknownSpan
            ),
            unknownSpan
        );

        var pipeline = CreatePipeline(
            [
                CreateParameter("serverName", ParameterType.Text, "TEST-SERVER")
            ],
            [
                CreateScriptStep("echo ok", unknownFields: [inputsField])
            ]
        );

        var result = CreateBinder().Bind(pipeline);

        var step = Assert.IsType<ScriptStepNode>(Assert.Single(result.Steps));
        var field = Assert.Single(step.UnknownFields);
        var mapping = Assert.IsType<UnknownMappingNode>(field.Value);
        var entry = Assert.Single(mapping.Entries);
        var value = Assert.IsType<UnknownScalarNode>(entry.Value);

        Assert.Equal("TEST-SERVER", value.Value);
    }

    [Fact]
    public void Bind_RootParameterReferencedInTemplateCall_ExpandsValues()
    {
        var pipeline = CreatePipeline(
            [
                CreateParameter("serverName", ParameterType.Text, "TEST-SERVER")
            ],
            [
                CreateTemplateStep(
                    "push-template.yaml",
                    new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["serverName"] = "${{ parameters.serverName }}"
                    }
                )
            ]
        );

        var result = CreateBinder().Bind(pipeline);

        var step = Assert.IsType<TemplateStepNode>(Assert.Single(result.Steps));

        Assert.Equal("TEST-SERVER", step.Parameters["serverName"].Value);
    }

    [Fact]
    public void Bind_CommandLineOverrideProvided_ExpandsOverrideValue()
    {
        var pipeline = CreatePipeline(
            [
                CreateParameter("serverName", ParameterType.Text, "TEST-SERVER")
            ],
            [
                CreateScriptStep("echo ${{ parameters.serverName }}")
            ]
        );

        var result = CreateBinder().Bind(
            pipeline,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["serverName"] = "TEST-SERVER"
            }
        );

        var step = Assert.IsType<ScriptStepNode>(Assert.Single(result.Steps));

        Assert.Equal("echo TEST-SERVER", step.Script.Value);
    }

    [Fact]
    public void Bind_CommandLineOverrideUnknownParameter_ThrowsException()
    {
        var pipeline = CreatePipeline(
            [
                CreateParameter("serverName", ParameterType.Text, "TEST-SERVER")
            ],
            [
                CreateScriptStep("echo ok")
            ]
        );

        var exception = Assert.Throws<PipelineParameterBindingException>(
            () => CreateBinder().Bind(
                pipeline,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["tenant"] = "contoso"
                }
            )
        );

        Assert.Equal("Parameter 'tenant' is not defined.", exception.Message);
    }

    [Fact]
    public void Bind_CommandLineOverrideWithInvalidBoolean_ThrowsException()
    {
        var pipeline = CreatePipeline(
            [
                CreateParameter("isEnabled", ParameterType.Boolean, "true")
            ],
            [
                CreateScriptStep("echo ok")
            ]
        );

        var exception = Assert.Throws<PipelineParameterBindingException>(
            () => CreateBinder().Bind(
                pipeline,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["isEnabled"] = "yes"
                }
            )
        );

        Assert.Equal("Parameter 'isEnabled' expects a boolean value but received 'yes'.", exception.Message);
    }

    [Fact]
    public void Bind_CommandLineOverrideOutsideAllowedValues_ThrowsException()
    {
        var pipeline = CreatePipeline(
            [
                CreateParameter("operatingSystem", ParameterType.Text, "ubuntu", ["ubuntu", "windows"])
            ],
            [
                CreateScriptStep("echo ok")
            ]
        );

        var exception = Assert.Throws<PipelineParameterBindingException>(
            () => CreateBinder().Bind(
                pipeline,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["operatingSystem"] = "linux"
                }
            )
        );

        Assert.Equal("Parameter 'operatingSystem' value 'linux' is not in the allowed values list.", exception.Message);
    }

    [Fact]
    public void Bind_ParameterValueOutsideAllowedValues_ThrowsException()
    {
        var pipeline = CreatePipeline(
            [
                CreateParameter("operatingSystem", ParameterType.Text, "ubuntu", ["ubuntu", "windows"])
            ],
            [
                CreateScriptStep("echo ok")
            ]
        );

        var exception = Assert.Throws<PipelineParameterBindingException>(() => CreateBinder().Bind(pipeline));

        Assert.Equal("Parameter 'operatingSystem' value 'linux' is not in the allowed values list.", exception.Message);
    }

    [Fact]
    public void Bind_ReferencedParameterWithoutValue_ThrowsException()
    {
        var pipeline = CreatePipeline(
            [
                CreateParameter("serverName", ParameterType.Text)
            ],
            [
                CreateScriptStep("echo ${{ parameters.serverName }}")
            ]
        );

        var exception = Assert.Throws<PipelineParameterBindingException>(() => CreateBinder().Bind(pipeline));

        Assert.Equal("Parameter 'serverName' does not have a value.", exception.Message);
    }

    [Fact]
    public void Bind_UnsupportedExpressionNamespace_ThrowsException()
    {
        var pipeline = CreatePipeline(
            [],
            [
                CreateScriptStep("echo ${{ variables.serverName }}")
            ]
        );

        var exception = Assert.Throws<PipelineParameterBindingException>(() => CreateBinder().Bind(pipeline));

        Assert.Equal("Unsupported expression 'variables.serverName'. Only 'parameters.<name>' is currently supported.", exception.Message);
    }

    private static IPipelineParameterBinder CreateBinder()
    {
        var services = new ServiceCollection();
        _ = services.AddPipelineParameterBinding();

        return services.BuildServiceProvider().GetRequiredService<IPipelineParameterBinder>();
    }

    private static PipelineNode CreatePipeline(
        IReadOnlyList<ParameterNode> parameters,
        IReadOnlyList<StepNode> steps,
        IReadOnlyList<UnknownFieldNode>? unknownFields = null,
        SourceSpan? span = null)
    {
        var effectiveSpan = span ?? CreateSpan();

        return new PipelineNode(
            parameters,
            steps,
            unknownFields ?? [],
            effectiveSpan
        );
    }

    private static ParameterNode CreateParameter(
        string name,
        ParameterType type,
        string? defaultValue = null,
        IReadOnlyList<string>? values = null,
        SourceSpan? span = null)
    {
        var effectiveSpan = span ?? CreateSpan();

        return new ParameterNode(
            name,
            null,
            type,
            defaultValue,
            values ?? [],
            [],
            effectiveSpan
        );
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

    private static TemplateStepNode CreateTemplateStep(
        string template,
        IReadOnlyDictionary<string, string>? parameters = null,
        IReadOnlyList<UnknownFieldNode>? unknownFields = null,
        SourceSpan? span = null)
    {
        var effectiveSpan = span ?? CreateSpan();
        var boundParameters = new Dictionary<string, InterpolatedStringNode>(StringComparer.Ordinal);

        if (parameters is not null)
        {
            foreach (var pair in parameters)
            {
                boundParameters[pair.Key] = new InterpolatedStringNode(pair.Value);
            }
        }

        return new TemplateStepNode(
            template,
            boundParameters,
            unknownFields ?? [],
            effectiveSpan
        );
    }

    private static SourceSpan CreateSpan(string sourceName = "application-test")
    {
        return SourceSpan.Unknown(new SourceRef(sourceName));
    }
}
