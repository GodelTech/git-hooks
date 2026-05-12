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
                CreateParameter("organization", ParameterType.Text, "godeltech")
            ],
            [
                CreateScriptStep(
                    "echo ${{ parameters.organization }}",
                    displayName: "Deploy ${{ parameters.organization }}",
                    workingDirectory: "./artifacts/${{ parameters.organization }}",
                    env: new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["ORG"] = "${{ parameters.organization }}"
                    }
                )
            ]
        );

        var result = CreateBinder().Bind(pipeline);

        var step = Assert.IsType<ScriptStepNode>(Assert.Single(result.Steps));

        Assert.Equal("echo godeltech", step.Script.Value);
        Assert.Equal("Deploy godeltech", step.DisplayName);
        Assert.Equal("./artifacts/godeltech", step.WorkingDirectory?.Value);
        Assert.Equal("godeltech", step.Env["ORG"].Value);
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
                        new UnknownScalarNode("${{ parameters.organization }}", unknownSpan),
                        unknownSpan
                    )
                ],
                unknownSpan
            ),
            unknownSpan
        );

        var pipeline = CreatePipeline(
            [
                CreateParameter("organization", ParameterType.Text, "godeltech")
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

        Assert.Equal("godeltech", value.Value);
    }

    [Fact]
    public void Bind_RootParameterReferencedInTemplateCall_ExpandsValues()
    {
        var pipeline = CreatePipeline(
            [
                CreateParameter("organization", ParameterType.Text, "godeltech")
            ],
            [
                CreateTemplateStep(
                    "deploy-template.yaml",
                    new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["organization"] = "${{ parameters.organization }}"
                    }
                )
            ]
        );

        var result = CreateBinder().Bind(pipeline);

        var step = Assert.IsType<TemplateStepNode>(Assert.Single(result.Steps));

        Assert.Equal("godeltech", step.Parameters["organization"].Value);
    }

    [Fact]
    public void Bind_CommandLineOverrideProvided_ExpandsOverrideValue()
    {
        var pipeline = CreatePipeline(
            [
                CreateParameter("organization", ParameterType.Text, "godeltech")
            ],
            [
                CreateScriptStep("echo ${{ parameters.organization }}")
            ]
        );

        var result = CreateBinder().Bind(
            pipeline,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["organization"] = "contoso"
            }
        );

        var step = Assert.IsType<ScriptStepNode>(Assert.Single(result.Steps));

        Assert.Equal("echo contoso", step.Script.Value);
    }

    [Fact]
    public void Bind_CommandLineOverrideUnknownParameter_ThrowsException()
    {
        var pipeline = CreatePipeline(
            [
                CreateParameter("organization", ParameterType.Text, "godeltech")
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
                CreateParameter("vmImage", ParameterType.Text, "ubuntu-latest", ["ubuntu-latest", "windows-latest"])
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
                    ["vmImage"] = "linux-latest"
                }
            )
        );

        Assert.Equal("Parameter 'vmImage' value 'linux-latest' is not in the allowed values list.", exception.Message);
    }

    [Fact]
    public void Bind_ParameterValueOutsideAllowedValues_ThrowsException()
    {
        var pipeline = CreatePipeline(
            [
                CreateParameter("vmImage", ParameterType.Text, "linux-latest", ["ubuntu-latest", "windows-latest"])
            ],
            [
                CreateScriptStep("echo ok")
            ]
        );

        var exception = Assert.Throws<PipelineParameterBindingException>(() => CreateBinder().Bind(pipeline));

        Assert.Equal("Parameter 'vmImage' value 'linux-latest' is not in the allowed values list.", exception.Message);
    }

    [Fact]
    public void Bind_ReferencedParameterWithoutValue_ThrowsException()
    {
        var pipeline = CreatePipeline(
            [
                CreateParameter("organization", ParameterType.Text)
            ],
            [
                CreateScriptStep("echo ${{ parameters.organization }}")
            ]
        );

        var exception = Assert.Throws<PipelineParameterBindingException>(() => CreateBinder().Bind(pipeline));

        Assert.Equal("Parameter 'organization' does not have a value.", exception.Message);
    }

    [Fact]
    public void Bind_UnsupportedExpressionNamespace_ThrowsException()
    {
        var pipeline = CreatePipeline(
            [],
            [
                CreateScriptStep("echo ${{ variables.organization }}")
            ]
        );

        var exception = Assert.Throws<PipelineParameterBindingException>(() => CreateBinder().Bind(pipeline));

        Assert.Equal("Unsupported expression 'variables.organization'. Only 'parameters.<name>' is currently supported.", exception.Message);
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
            Env = env?.ToDictionary(
                pair => pair.Key,
                pair => new InterpolatedStringNode(pair.Value),
                StringComparer.Ordinal
            ) ?? new Dictionary<string, InterpolatedStringNode>(StringComparer.Ordinal)
        };
    }

    private static TemplateStepNode CreateTemplateStep(
        string template,
        IReadOnlyDictionary<string, string>? parameters = null,
        IReadOnlyList<UnknownFieldNode>? unknownFields = null,
        SourceSpan? span = null)
    {
        var effectiveSpan = span ?? CreateSpan();

        return new TemplateStepNode(
            template,
            parameters?.ToDictionary(
                pair => pair.Key,
                pair => new InterpolatedStringNode(pair.Value),
                StringComparer.Ordinal
            ) ?? new Dictionary<string, InterpolatedStringNode>(StringComparer.Ordinal),
            unknownFields ?? [],
            effectiveSpan
        );
    }

    private static SourceSpan CreateSpan(string sourceName = "application-test")
    {
        return SourceSpan.Unknown(new SourceRef(sourceName));
    }
}
