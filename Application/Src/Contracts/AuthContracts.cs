namespace Api.Application;

public sealed record RegisteredUserResponse(Guid UserId, string Email, string UserName, DateTime CreatedAtUtc);

public sealed record TokenResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    UserProfileResponse User);

public sealed record UserProfileResponse(
    Guid Id,
    string Email,
    string UserName,
    Guid? TenantId,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);

public sealed record RoleResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    bool IsSystem,
    Guid? TenantId);

public sealed record PermissionResponse(
    Guid Id,
    string Code,
    string Name,
    string Resource,
    string Action,
    string? Description,
    bool IsActive);

public sealed record PermissionCheckResponse(Guid UserId, string PermissionCode, bool Granted);