using SupportAdvance.Contexts.Identity.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Identity.Domain.Tests.ValueObjects;

public class LoginCredentialsRowIdTests
{
    [Fact]
    public void From_WithValidValue_ReturnsInstance()
    {
        var rowId = LoginCredentialsRowId.From(1);
        Assert.NotNull(rowId);
        Assert.Equal(1L, rowId.Value);
    }

    [Fact]
    public void From_WithZero_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => LoginCredentialsRowId.From(0));
        Assert.Contains("must be 1 or higher", exception.Message);
    }

    [Fact]
    public void From_WithNegativeValue_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => LoginCredentialsRowId.From(-1));
        Assert.Contains("must be 1 or higher", exception.Message);
    }

    [Fact]
    public void TryFrom_WithValidValue_ReturnsTrue()
    {
        var result = LoginCredentialsRowId.TryFrom(50, out var rowId);
        Assert.True(result);
        Assert.Equal(50L, rowId.Value);
    }

    [Fact]
    public void TryFrom_WithZero_ReturnsFalse()
    {
        var result = LoginCredentialsRowId.TryFrom(0, out var rowId);
        Assert.False(result);
        Assert.Null(rowId);
    }

    [Fact]
    public void TryFromDbValue_WithValidValue_ReturnsTrue()
    {
        var result = LoginCredentialsRowId.TryFromDbValue(200, out var rowId);
        Assert.True(result);
        Assert.Equal(200L, rowId.Value);
    }

    [Fact]
    public void TryFromDbValue_WithZero_ReturnsFalse()
    {
        var result = LoginCredentialsRowId.TryFromDbValue(0, out var rowId);
        Assert.False(result);
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        var rowId1 = LoginCredentialsRowId.From(25);
        var rowId2 = LoginCredentialsRowId.From(25);
        Assert.Equal(rowId1, rowId2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ReturnsFalse()
    {
        var rowId1 = LoginCredentialsRowId.From(25);
        var rowId2 = LoginCredentialsRowId.From(26);
        Assert.NotEqual(rowId1, rowId2);
    }

    [Fact]
    public void ToString_ReturnsStringValue()
    {
        var rowId = LoginCredentialsRowId.From(123);
        Assert.Equal("123", rowId.ToString());
    }

    [Fact]
    public void EqualityOperator_WithSameValue_ReturnsTrue()
    {
        var rowId1 = LoginCredentialsRowId.From(8);
        var rowId2 = LoginCredentialsRowId.From(8);
        Assert.True(rowId1 == rowId2);
    }

    [Fact]
    public void InequalityOperator_WithDifferentValue_ReturnsTrue()
    {
        var rowId1 = LoginCredentialsRowId.From(8);
        var rowId2 = LoginCredentialsRowId.From(9);
        Assert.True(rowId1 != rowId2);
    }
}
