namespace Shared.Exceptions;

public sealed class ForbiddenException(string message) : AppException(message, 403);