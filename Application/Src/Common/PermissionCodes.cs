namespace Application.Common;

public static class PermissionCodes
{
    public const string RolesRead = "auth.roles.read";
    public const string RolesManage = "auth.roles.manage";
    public const string PermissionsRead = "auth.permissions.read";
    public const string PermissionsManage = "auth.permissions.manage";
    public const string UsersRead = "auth.users.read";
    public const string UsersManage = "auth.users.manage";
    public const string TokensRevoke = "auth.tokens.revoke";

    public static readonly string[] All =
    [
        RolesRead,
        RolesManage,
        PermissionsRead,
        PermissionsManage,
        UsersRead,
        UsersManage,
        TokensRevoke
    ];
}