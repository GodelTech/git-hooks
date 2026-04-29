namespace GitHooks.Workflow.Domain.Model;

public readonly record struct StepId(string Value)
{
    public override string ToString()
    {
        return Value;
    }

    public static StepId New()
    {
        return new(Guid.NewGuid().ToString());
    }
}
