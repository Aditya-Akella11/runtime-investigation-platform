namespace RuntimeInvestigation.Shared.Results;

public record Error(string Code, string Message);

public class Result
{
    protected Result(bool isSuccess, Error? error) { IsSuccess = isSuccess; Error = error; }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }
    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error ?? throw new ArgumentNullException(nameof(error)));
}

public sealed class Result<T> : Result
{
    private Result(bool success, T? value, Error? error) : base(success, error) => Value = value;
    public T? Value { get; }
    public static Result<T> Success(T value) => new(true, value, null);
    public static new Result<T> Failure(Error error) => new(false, default, error);
}
