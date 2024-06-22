using Tests.Setup.Utils.Comparers;

namespace Tests.Tests.Utils.Comparers;

public sealed class DoubleComparerTests
{
    [Theory]
    [InlineData(0.1, 0.2, 0.3)]
    [InlineData(0.23, 0.43, 0.66)]
    public void Compare_ComparesTwoDoubles_True(double first, double second, double sumExpected)
    {
        // Arrange
        var sum = first + second;

        // Act
        var isEqual = DoubleComparer.Compare(sum, sumExpected);

        // Assert
        Assert.True(isEqual);
    }

    [Theory]
    [InlineData(0.1, 0.2, 0.29)]
    [InlineData(0.1, 0.2, 0.31)]
    [InlineData(0.23, 0.43, 0.67)]
    [InlineData(0.23, 0.43, 0.65)]
    [InlineData(0.23, 0.43, 0.660000001)]
    public void Compare_ComparesTwoDoubles_False(double first, double second, double sumExpected)
    {
        // Arrange
        var sum = first + second;

        // Act
        var isEqual = DoubleComparer.Compare(sum, sumExpected);

        // Assert
        Assert.False(isEqual);
    }
}
