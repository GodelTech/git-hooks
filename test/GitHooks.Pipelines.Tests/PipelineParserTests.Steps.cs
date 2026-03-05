using GitHooks.Pipelines.Models;

namespace GitHooks.Pipelines.Tests;

public partial class PipelineParserTests
{
    public static TheoryData<string, Pipeline> StepsValidYamlCases => new()
    {
        {
            """
            steps:
            - task: test-task
            """,
            new Pipeline
            {
                Steps = new List<Step>
                {
                    new Step
                    {
                        Task = "test-task"
                    }
                }
            }
        },
        {
            """
            steps:
            - task: test-task
              name: TestName
              displayName: 'Test display name'
              enabled: true
              continueOnError: false
              inputs:
                key1: value1
            """,
            new Pipeline
            {
                Steps = new List<Step>
                {
                    new Step
                    {
                        Task = "test-task",
                        Name = "TestName",
                        DisplayName = "Test display name",
                        Enabled = true,
                        ContinueOnError = false,
                        Inputs = new Dictionary<string, object>
                        {
                            { "key1", "value1" }
                        }
                    }
                }
            }
        },
        {
            """
            steps:
            - task: test-task
            - task: test-second-task
            """,
            new Pipeline
            {
                Steps = new List<Step>
                {
                    new Step
                    {
                        Task = "test-task"
                    },
                    new Step
                    {
                        Task = "test-second-task"
                    }
                }
            }
        }
    };
}
