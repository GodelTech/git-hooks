using GitHooks.Domain.Ast;
using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Fields;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Values;
using GitHooks.Domain.Ast.Visitors;

namespace GitHooks.Domain.Tests.Ast;

public sealed class AstNodeVisitorDispatchTests
{
    [Fact]
    public void Accept_CommandVisitor_RoutesToExpectedMethod()
    {
        foreach (var (node, _, expectedVisit) in TestAstFactory.NodeCases)
        {
            var visitor = new TrackingCommandVisitor();

            node.Accept(visitor);

            Assert.Equal(expectedVisit, visitor.LastVisited);
        }
    }

    [Fact]
    public void Accept_QueryVisitor_RoutesToExpectedMethod()
    {
        foreach (var (node, _, expectedVisit) in TestAstFactory.NodeCases)
        {
            var visitor = new TrackingQueryVisitor();

            var result = node.Accept(visitor);

            Assert.Equal(expectedVisit, result);
        }
    }

    private sealed class TrackingCommandVisitor : IAstCommandVisitor
    {
        public string LastVisited { get; private set; } = string.Empty;

        public void Visit(PipelineNode node)
        {
            LastVisited = nameof(PipelineNode);
        }

        public void Visit(ParameterNode node)
        {
            LastVisited = nameof(ParameterNode);
        }

        public void Visit(ScriptStepNode node)
        {
            LastVisited = nameof(ScriptStepNode);
        }

        public void Visit(TemplateStepNode node)
        {
            LastVisited = nameof(TemplateStepNode);
        }

        public void Visit(InvalidStepNode node)
        {
            LastVisited = nameof(InvalidStepNode);
        }

        // Fields
        public void Visit<TValue>(StringKeyFieldNode<TValue> node)
            where TValue : AstNode
        {
            LastVisited = nameof(StringKeyFieldNode<>);
        }

        public void Visit<TValue>(ComplexKeyFieldNode<TValue> node)
            where TValue : AstNode
        {
            LastVisited = nameof(ComplexKeyFieldNode<>);
        }

        public void Visit<TValue>(SequenceFieldNode<TValue> node)
            where TValue : AstNode
        {
            LastVisited = nameof(SequenceFieldNode<>);
        }

        public void Visit<TField>(MappingFieldNode<TField> node)
            where TField : FieldNode
        {
            LastVisited = nameof(MappingFieldNode<>);
        }

        // Values
        public void Visit(ScalarNode node)
        {
            LastVisited = nameof(ScalarNode);
        }

        public void Visit(SequenceNode node)
        {
            LastVisited = nameof(SequenceNode);
        }

        public void Visit(MappingNode node)
        {
            LastVisited = nameof(MappingNode);
        }

        // Expressions
        public void Visit(BooleanLiteralExpressionNode node)
        {
            LastVisited = nameof(BooleanLiteralExpressionNode);
        }

        public void Visit(IntegerLiteralExpressionNode node)
        {
            LastVisited = nameof(IntegerLiteralExpressionNode);
        }

        public void Visit(StringLiteralExpressionNode node)
        {
            LastVisited = nameof(StringLiteralExpressionNode);
        }

        public void Visit(InterpolatedStringExpressionNode node)
        {
            LastVisited = nameof(InterpolatedStringExpressionNode);
        }

        public void Visit(VariableExpressionNode node)
        {
            LastVisited = nameof(VariableExpressionNode);
        }
    }

    private sealed class TrackingQueryVisitor : IAstQueryVisitor<string>
    {
        public string Visit(PipelineNode node)
        {
            return nameof(PipelineNode);
        }

        public string Visit(ParameterNode node)
        {
            return nameof(ParameterNode);
        }

        public string Visit(ScriptStepNode node)
        {
            return nameof(ScriptStepNode);
        }

        public string Visit(TemplateStepNode node)
        {
            return nameof(TemplateStepNode);
        }

        public string Visit(InvalidStepNode node)
        {
            return nameof(InvalidStepNode);
        }

        // Fields
        public string Visit<TValue>(StringKeyFieldNode<TValue> node)
            where TValue : AstNode
        {
            return nameof(StringKeyFieldNode<>);
        }

        public string Visit<TValue>(ComplexKeyFieldNode<TValue> node)
            where TValue : AstNode
        {
            return nameof(ComplexKeyFieldNode<>);
        }

        public string Visit<TValue>(SequenceFieldNode<TValue> node)
            where TValue : AstNode
        {
            return nameof(SequenceFieldNode<>);
        }

        public string Visit<TField>(MappingFieldNode<TField> node)
            where TField : FieldNode
        {
            return nameof(MappingFieldNode<>);
        }

        // Values
        public string Visit(ScalarNode node)
        {
            return nameof(ScalarNode);
        }

        public string Visit(SequenceNode node)
        {
            return nameof(SequenceNode);
        }

        public string Visit(MappingNode node)
        {
            return nameof(MappingNode);
        }

        // Expressions
        public string Visit(BooleanLiteralExpressionNode node)
        {
            return nameof(BooleanLiteralExpressionNode);
        }

        public string Visit(IntegerLiteralExpressionNode node)
        {
            return nameof(IntegerLiteralExpressionNode);
        }

        public string Visit(StringLiteralExpressionNode node)
        {
            return nameof(StringLiteralExpressionNode);
        }

        public string Visit(InterpolatedStringExpressionNode node)
        {
            return nameof(InterpolatedStringExpressionNode);
        }

        public string Visit(VariableExpressionNode node)
        {
            return nameof(VariableExpressionNode);
        }
    }
}
