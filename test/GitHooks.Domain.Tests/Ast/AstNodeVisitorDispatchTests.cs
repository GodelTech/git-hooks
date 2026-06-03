using GitHooks.Domain.Ast.Expressions;
using GitHooks.Domain.Ast.Mappings;
using GitHooks.Domain.Ast.Mappings.Parameters;
using GitHooks.Domain.Ast.Mappings.Steps;
using GitHooks.Domain.Ast.Unknown;
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

        public void Visit(VariableExpressionNode node)
        {
            LastVisited = nameof(VariableExpressionNode);
        }

        public void Visit(InterpolatedStringExpressionNode node)
        {
            LastVisited = nameof(InterpolatedStringExpressionNode);
        }

        public void VisitUnknownNode(UnknownNode node)
        {
            LastVisited = nameof(UnknownNode);
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

        public string Visit(VariableExpressionNode node)
        {
            return nameof(VariableExpressionNode);
        }

        public string Visit(InterpolatedStringExpressionNode node)
        {
            return nameof(InterpolatedStringExpressionNode);
        }

        public string VisitUnknownNode(UnknownNode node)
        {
            return nameof(UnknownNode);
        }
    }
}
