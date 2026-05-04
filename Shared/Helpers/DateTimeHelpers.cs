namespace Auth.Shared.Helpers;

public static class DateTimeHelper
{
    public const string SqlUtcNow = "GETUTCDATE()";

    public static DateTime UtcNow()
    {
        return DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }
}