using Xunit;

namespace Integration.Tests.Fixtures;

/// <summary>
/// Base class for integration tests with database context
/// Provides common setup and teardown functionality
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    public virtual Task InitializeAsync()
    {
        // Setup test database, migrations, etc.
        return Task.CompletedTask;
    }

    public virtual Task DisposeAsync()
    {
        // Cleanup resources
        return Task.CompletedTask;
    }
}