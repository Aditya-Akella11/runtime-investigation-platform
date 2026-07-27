namespace RuntimeInvestigation.Shared.Logging;
public interface IPlatformLogger<in T>
{
    void Information(string message, params object?[] args);
    void Error(Exception exception, string message, params object?[] args);
}
