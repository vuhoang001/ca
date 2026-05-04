using Xunit;

namespace Domain.UnitTests;

public class DomainTestExample
{
    [Fact]
    public void Sample_Test_Should_Pass()
    {
        // Arrange
        var expectedValue = 5;

        // Act
        var result = 2 + 3;

        // Assert
        Assert.Equal(expectedValue, result);
    }
}


