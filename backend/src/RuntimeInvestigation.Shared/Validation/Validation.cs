using RuntimeInvestigation.Shared.Results;
namespace RuntimeInvestigation.Shared.Validation;
public interface IValidator<in T> { IReadOnlyList<Error> Validate(T value); }
public static class Guard
{
    public static string Required(string? value, string parameterName) => !string.IsNullOrWhiteSpace(value) ? value.Trim() : throw new Exceptions.ValidationException($"{parameterName} is required.");
}
