namespace Infrastructure.Options;

public sealed class RabbitMqOptions
{
    public const string Section = "RabbitMq";

    /// <summary>
    /// Để trống → dùng InMemory transport (không cần broker).
    /// Điền host (vd: "localhost") → tự động kết nối RabbitMQ.
    /// </summary>
    public string Host { get; init; } = string.Empty;
    public ushort Port { get; init; } = 5672;
    public string VirtualHost { get; init; } = "/";
    public string Username { get; init; } = "guest";
    public string Password { get; init; } = "guest";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host);
}
