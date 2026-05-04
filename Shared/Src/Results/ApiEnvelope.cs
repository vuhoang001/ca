namespace Shared.Results;

public sealed record ApiEnvelope<T>(T Data, string? Message = null);
