namespace GitHooks.Pipelines.Models;

public sealed class ParseResult<T>
{
    private ParseResult(T value)
    {
        IsSuccess = true;
        Value = value;
        Errors = [];
    }

    private ParseResult(IReadOnlyList<ParseError> errors)
    {
        IsSuccess = false;
        Value = default;
        Errors = errors;
    }

    public bool IsSuccess { get; }
    public T? Value { get; }
    public IReadOnlyList<ParseError> Errors { get; }

    public static ParseResult<T> Ok(T value) => new(value);

    public static ParseResult<T> Fail(IReadOnlyList<ParseError> errors) => new(errors);
}
