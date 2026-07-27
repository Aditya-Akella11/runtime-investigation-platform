namespace RuntimeInvestigation.Shared.Exceptions;
public abstract class PlatformException(string message) : Exception(message);
public sealed class ValidationException(string message) : PlatformException(message);
public sealed class NotFoundException(string resource, string id) : PlatformException($"{resource} '{id}' was not found.");
