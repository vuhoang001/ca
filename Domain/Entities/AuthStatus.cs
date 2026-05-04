namespace Auth.Domain.Entities;

public class AuthStatus
{
    public string Value { get; }

    private AuthStatus(string value)
    {
        Value = value;
    }

    private AuthStatus()
    {
    }

    public static AuthStatus Active  = new("Active");
    public static AuthStatus Locked  = new("Locked");
    public static AuthStatus Deleted = new("Deleted");

    public bool CanLogin() => this == Active;
}