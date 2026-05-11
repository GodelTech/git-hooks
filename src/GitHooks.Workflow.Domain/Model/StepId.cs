namespace GitHooks.Workflow.Domain.Model;

public readonly record struct StepId(string Value)
{
    public override string ToString()
    {
        return Value;
    }

    public static StepId New()
    {
        // TODO: I don't like this, but I don't want to add a dependency on System.Guid in the domain model. Maybe we can use a different approach for generating unique IDs in the future.
        return new(Guid.NewGuid().ToString());
    }
}

