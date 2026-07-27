using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Values;
using GitHooks.Domain.Ast.Visitors;
using GitHooks.Testing.Ast.Builders.Expressions;
using GitHooks.Testing.Ast.Builders.Fields;
using GitHooks.Testing.Ast.Builders.Mappings;
using GitHooks.Testing.Ast.Builders.Mappings.Parameters;
using GitHooks.Testing.Ast.Builders.Mappings.Steps;
using GitHooks.Testing.Ast.Builders.Values;

namespace GitHooks.Domain.Tests.Ast.Visitors;

public sealed class AstWalkerTests
{
    [Fact]
    public void Walk_NullRoot_Throws()
    {
        var walker = new TestAstWalker();

        var exception = Assert.Throws<ArgumentNullException>(
            () => walker.ExposedWalk(null!));

        Assert.Equal(
            "root",
            exception.ParamName);
    }

    [Fact]
    public void Visit_PipelineNode_VisitsParametersStepsAndUnknownFields()
    {
        var pipeline = new PipelineNodeBuilder()
            .WithParameter(x => x
                .WithName("configuration"))
            .WithScriptStep(x => x
                .WithScript("dotnet test"))
            .WithUnknownField("custom", "value")
            .WithUnknownStringKeyField(x => x
                .WithKey("unknownField2")
                .WithBooleanValue(b => b.WithValue(true)))
            .Build();

        var walker = new TestAstWalker();

        walker.ExposedWalk(pipeline);

        Assert.Equal(
            [
                "Pipeline",
                "Parameter",
                "Field(name)",
                "String(configuration)",
                "ScriptStep",
                "Field(script)",
                "String(dotnet test)",
                "UnknownFields",
                "Field(custom)",
                "String(value)",
                "Field(unknownField2)",
                "Boolean(True)"
            ],
            walker.Visited);
    }

    [Fact]
    public void Visit_ParameterNode_VisitsFields()
    {
        var parameter = new ParameterNodeBuilder()
            .WithName("configuration")
            .WithDisplayName("Configuration")
            .WithType("string")
            .WithDefaultValue("Release")
            .WithValue("Debug")
            .WithValue("Release")
            .WithUnknownField("custom", "value")
            .Build();

        var walker = new TestAstWalker();

        walker.ExposedWalk(parameter);

        Assert.Equal(
            [
                "Parameter",
                "Field(name)",
                "String(configuration)",
                "Field(displayName)",
                "String(Configuration)",
                "Field(type)",
                "String(string)",
                "Field(defaultValue)",
                "String(Release)",
                "SequenceField(values)",
                "String(Debug)",
                "String(Release)",
                "UnknownFields",
                "Field(custom)",
                "String(value)"
            ],
            walker.Visited);
    }

    [Fact]
    public void Visit_ScriptStepNode_VisitsFields()
    {
        var step = new ScriptStepNodeBuilder()
            .WithScript("dotnet test")
            .WithDisplayName("Tests")
            .WithCondition("succeeded()")
            .WithTimeoutInMinutes(30)
            .WithWorkingDirectory("src/")
            .WithEnvironmentVariable("Configuration", "Release")
            .WithUnknownField("custom", "value")
            .Build();

        var walker = new TestAstWalker();

        walker.ExposedWalk(step);

        Assert.Equal(
            [
                "ScriptStep",
                "Field(script)",
                "String(dotnet test)",
                "Field(displayName)",
                "String(Tests)",
                "Field(condition)",
                "String(succeeded())",
                "Field(timeoutInMinutes)",
                "Integer(30)",
                "Field(workingDirectory)",
                "String(src/)",
                "MappingField(env)",
                "Field(Configuration)",
                "String(Release)",
                "UnknownFields",
                "Field(custom)",
                "String(value)"
            ],
            walker.Visited);
    }

    [Fact]
    public void Visit_TemplateStepNode_VisitsFields()
    {
        var step = new TemplateStepNodeBuilder()
            .WithTemplate("build.yml")
            .WithParameter("configuration", "Release")
            .WithUnknownField("custom", "value")
            .Build();

        var walker = new TestAstWalker();

        walker.ExposedWalk(step);

        Assert.Equal(
            [
                "TemplateStep",
                "Field(template)",
                "String(build.yml)",
                "MappingField(parameters)",
                "Field(configuration)",
                "String(Release)",
                "UnknownFields",
                "Field(custom)",
                "String(value)"
            ],
            walker.Visited);
    }

    [Fact]
    public void Visit_InvalidStepNode_VisitsFields()
    {
        var step = new InvalidStepNodeBuilder()
            .WithField("script", "dotnet test")
            .WithUnknownField("custom", "value")
            .Build();

        var walker = new TestAstWalker();

        walker.ExposedWalk(step);

        Assert.Equal(
            [
                "InvalidStep",
                "Field(script)",
                "String(dotnet test)",
                "UnknownFields",
                "Field(custom)",
                "String(value)"
            ],
            walker.Visited);
    }

    [Fact]
    public void Visit_ComplexKeyFieldNode_VisitsFields()
    {
        var field = new ComplexKeyFieldNodeBuilder()
            .WithKey("complexKey")
            .WithSequenceValue(x => x.WithItem("firstItem"))
            .Build();

        var walker = new TestAstWalker();

        walker.ExposedWalk(field);

        Assert.Equal(
            [
                "ComplexField",
                "ComplexFieldKey",
                "Scalar(complexKey)",
                "ComplexFieldValue",
                "Sequence",
                "Scalar(firstItem)"
            ],
            walker.Visited);
    }

    [Fact]
    public void Visit_SequenceNode_VisitsItems()
    {
        var sequence = new SequenceNodeBuilder()
            .WithItem("Debug")
            .WithItem("Release")
            .Build();

        var walker = new TestAstWalker();

        walker.ExposedWalk(sequence);

        Assert.Equal(
            [
                "Sequence",
                "Scalar(Debug)",
                "Scalar(Release)"
            ],
            walker.Visited);
    }

    [Fact]
    public void Visit_MappingNode_VisitsFields()
    {
        var mapping = new MappingNodeBuilder()
            .WithField("configuration", "Release")
            .Build();

        var walker = new TestAstWalker();

        walker.ExposedWalk(mapping);

        Assert.Equal(
            [
                "Mapping",
                "Field(configuration)",
                "String(Release)"
            ],
            walker.Visited);
    }

    [Fact]
    public void Visit_InterpolatedStringExpressionNode_VisitsParts()
    {
        var expression = new InterpolatedStringExpressionNodeBuilder()
            .WithPart("Hello ")
            .WithParameterVariablePart("configuration")
            .WithInvalidVariablePart("foo.bar")
            .Build();

        var walker = new TestAstWalker();

        walker.ExposedWalk(expression);

        Assert.Equal(
            [
                "InterpolatedString",
                "String(Hello )",
                "ParameterVariable(configuration)",
                "InvalidVariable(foo.bar)"
            ],
            walker.Visited);
    }

    private sealed class TestAstWalker : AstWalker
    {
        public List<string> Visited { get; }
            = [];

        public void ExposedWalk(
            AstNode node)
        {
            Walk(node);
        }

        public override void Visit(
            PipelineNode node)
        {
            Visited.Add("Pipeline");
            base.Visit(node);
        }

        public override void Visit(
            ParameterNode node)
        {
            Visited.Add("Parameter");
            base.Visit(node);
        }

        public override void Visit(
            ScriptStepNode node)
        {
            Visited.Add("ScriptStep");
            base.Visit(node);
        }

        public override void Visit(
            TemplateStepNode node)
        {
            Visited.Add("TemplateStep");
            base.Visit(node);
        }

        public override void Visit(
            InvalidStepNode node)
        {
            Visited.Add("InvalidStep");
            base.Visit(node);
        }

        // Fields
        public override void Visit<TValue>(
            StringKeyFieldNode<TValue> node)
        {
            Visited.Add($"Field({node.Key})");
            base.Visit(node);
        }

        public override void Visit<TValue>(
            ComplexKeyFieldNode<TValue> node)
        {
            Visited.Add("ComplexField");
            base.Visit(node);
        }

        public override void Visit<TValue>(
            SequenceFieldNode<TValue> node)
        {
            Visited.Add($"SequenceField({node.Key})");
            base.Visit(node);
        }

        public override void Visit<TField>(
            MappingFieldNode<TField> node)
        {
            Visited.Add($"MappingField({node.Key})");
            base.Visit(node);
        }

        // Values
        public override void Visit(
            ScalarNode node)
        {
            Visited.Add($"Scalar({node.Value})");
            base.Visit(node);
        }

        public override void Visit(
            SequenceNode node)
        {
            Visited.Add("Sequence");
            base.Visit(node);
        }

        public override void Visit(
            MappingNode node)
        {
            Visited.Add("Mapping");
            base.Visit(node);
        }

        // Expressions
        public override void Visit(
            BooleanLiteralExpressionNode node)
        {
            Visited.Add($"Boolean({node.Value})");
            base.Visit(node);
        }

        public override void Visit(
            IntegerLiteralExpressionNode node)
        {
            Visited.Add($"Integer({node.Value})");
            base.Visit(node);
        }

        public override void Visit(
            StringLiteralExpressionNode node)
        {
            Visited.Add($"String({node.Value})");
            base.Visit(node);
        }

        public override void Visit(
            InterpolatedStringExpressionNode node)
        {
            Visited.Add("InterpolatedString");
            base.Visit(node);
        }

        public override void Visit(
            ParameterVariableExpressionNode node)
        {
            Visited.Add($"ParameterVariable({node.Name})");
            base.Visit(node);
        }

        public override void Visit(
            InvalidVariableExpressionNode node)
        {
            Visited.Add($"InvalidVariable({node.Text})");
            base.Visit(node);
        }

        protected override void WalkUnknownFields(
            PipelineNodeBase node)
        {
            if (node.UnknownFields.Count > 0)
            {
                Visited.Add("UnknownFields");
            }

            base.WalkUnknownFields(node);
        }

        protected override void WalkComplexFieldKey<T>(
            ComplexKeyFieldNode<T> node)
        {
            Visited.Add("ComplexFieldKey");

            base.WalkComplexFieldKey(node);
        }

        protected override void WalkComplexFieldValue<T>(
            ComplexKeyFieldNode<T> node)
        {
            Visited.Add("ComplexFieldValue");

            base.WalkComplexFieldValue(node);
        }
    }
}
