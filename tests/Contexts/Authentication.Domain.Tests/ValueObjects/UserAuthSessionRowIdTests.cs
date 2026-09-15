using SupportAdvance.Contexts.Identity.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Identity.Domain.Tests.ValueObjects;

public class UserAuthSessionRowIdTests
{
    [Fact]
    public void From_WithValidValue_ReturnsInstance()
    {
        var rowId = UserAuthSessionRowId.From(1);
        Assert.NotNull(rowId);
        Assert.Equal(1L, rowId.Value);
    }

    [Fact]
    public void From_WithZero_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => UserAuthSessionRowId.From(0));
        Assert.Contains("must be 1 or higher", exception.Message);
    }

    [Fact]
    public void From_WithNegativeValue_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => UserAuthSessionRowId.From(-1));
        Assert.Contains("must be 1 or higher", exception.Message);
    }

    [Fact]
    public void TryFrom_WithValidValue_ReturnsTrue()
    {
        var result = UserAuthSessionRowId.TryFrom(100, out var rowId);
        Assert.True(result);
        Assert.Equal(100L, rowId.Value);
    }

    [Fact]
    public void TryFrom_WithZero_ReturnsFalse()
    {
        var result = UserAuthSessionRowId.TryFrom(0, out var rowId);
        Assert.False(result);
        Assert.Null(rowId);
    }

    [Fact]
    public void TryFrom_WithNegativeValue_ReturnsFalse()
    {
        var result = UserAuthSessionRowId.TryFrom(-5, out var rowId);
        Assert.False(result);
        Assert.Null(rowId);
    }

    [Fact]
    public void TryFromDbValue_WithValidValue_ReturnsTrue()
    {
        var result = UserAuthSessionRowId.TryFromDbValue(999, out var rowId);
        Assert.True(result);
        Assert.Equal(999L, rowId.Value);
    }

    [Fact]
    public void TryFromDbValue_WithZero_ReturnsFalse()
    {
        var result = UserAuthSessionRowId.TryFromDbValue(0, out var rowId);
        Assert.False(result);
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        var rowId1 = UserAuthSessionRowId.From(5);
        var rowId2 = UserAuthSessionRowId.From(5);
        Assert.Equal(rowId1, rowId2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ReturnsFalse()
    {
        var rowId1 = UserAuthSessionRowId.From(5);
        var rowId2 = UserAuthSessionRowId.From(10);
        Assert.NotEqual(rowId1, rowId2);
    }

    [Fact]
    public void GetHashCode_WithSameValue_ReturnsSameHashCode()
    {
        var rowId1 = UserAuthSessionRowId.From(7);
        var rowId2 = UserAuthSessionRowId.From(7);
        Assert.Equal(rowId1.GetHashCode(), rowId2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsStringValue()
    {
        var rowId = UserAuthSessionRowId.From(42);
        Assert.Equal("42", rowId.ToString());
    }

    [Fact]
    public void EqualityOperator_WithSameValue_ReturnsTrue()
    {
        var rowId1 = UserAuthSessionRowId.From(3);
        var rowId2 = UserAuthSessionRowId.From(3);
        Assert.True(rowId1 == rowId2);
    }

    [Fact]
    public void InequalityOperator_WithDifferentValue_ReturnsTrue()
    {
        var rowId1 = UserAuthSessionRowId.From(3);
        var rowId2 = UserAuthSessionRowId.From(4);
        Assert.True(rowId1 != rowId2);
    }
}
