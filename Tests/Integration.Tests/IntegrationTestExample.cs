using Xunit;

namespace Integration.Tests;

public class IntegrationTestExample
{
    [Fact]
    public void Sample_Integration_Test_Should_Pass()
    {
        // Arrange
        var expectedValue = 15;

        // Act
        var result = 10 + 5;

        // Assert
        Assert.Equal(expectedValue, result);
    }
}