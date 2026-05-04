using Xunit;

namespace Application.UnitTests;

public class ApplicationTestExample
{
    [Fact]
    public void Sample_Test_Should_Pass()
    {
        // Arrange
        var expectedValue = 10;
        
        // Act
        var result = 5 * 2;
        
        // Assert
        Assert.Equal(expectedValue, result);
    }
}


