namespace Shared.Exceptions;

public sealed class BadRequestException(string message) : AppException(message, 400);
