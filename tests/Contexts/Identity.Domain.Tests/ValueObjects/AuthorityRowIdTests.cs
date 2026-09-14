using SupportAdvance.Contexts.Identity.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Identity.Domain.Tests.ValueObjects;

public class AuthorityRowIdTests
{
    [Fact]
    public void From_WithValidValue_ReturnsInstance()
    {
        var rowId = AuthorityRowId.From(1);
        Assert.NotNull(rowId);
        Assert.Equal(1L, rowId.Value);
    }

    [Fact]
    public void From_WithZero_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => AuthorityRowId.From(0));
        Assert.Contains("must be 1 or higher", exception.Message);
    }

    [Fact]
    public void From_WithNegativeValue_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => AuthorityRowId.From(-10));
        Assert.Contains("must be 1 or higher", exception.Message);
    }

    [Fact]
    public void TryFrom_WithValidValue_ReturnsTrue()
    {
        var result = AuthorityRowId.TryFrom(1000, out var rowId);
        Assert.True(result);
        Assert.Equal(1000L, rowId.Value);
    }

    [Fact]
    public void TryFrom_WithZero_ReturnsFalse()
    {
        var result = AuthorityRowId.TryFrom(0, out var rowId);
        Assert.False(result);
        Assert.Null(rowId);
    }

    [Fact]
    public void TryFromDbValue_WithValidValue_ReturnsTrue()
    {
        var result = AuthorityRowId.TryFromDbValue(2147483647, out var rowId);
        Assert.True(result);
        Assert.Equal(2147483647L, rowId.Value);
    }

    [Fact]
    public void TryFromDbValue_WithZero_ReturnsFalse()
    {
        var result = AuthorityRowId.TryFromDbValue(0, out var rowId);
        Assert.False(result);
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        var rowId1 = AuthorityRowId.From(99);
        var rowId2 = AuthorityRowId.From(99);
        Assert.Equal(rowId1, rowId2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ReturnsFalse()
    {
        var rowId1 = AuthorityRowId.From(99);
        var rowId2 = AuthorityRowId.From(100);
        Assert.NotEqual(rowId1, rowId2);
    }

    [Fact]
    public void GetHashCode_WithSameValue_ReturnsSameHashCode()
    {
        var rowId1 = AuthorityRowId.From(55);
        var rowId2 = AuthorityRowId.From(55);
        Assert.Equal(rowId1.GetHashCode(), rowId2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsStringValue()
    {
        var rowId = AuthorityRowId.From(777);
        Assert.Equal("777", rowId.ToString());
    }

    [Fact]
    public void EqualityOperator_WithSameValue_ReturnsTrue()
    {
        var rowId1 = AuthorityRowId.From(2);
        var rowId2 = AuthorityRowId.From(2);
        Assert.True(rowId1 == rowId2);
    }

    [Fact]
    public void InequalityOperator_WithDifferentValue_ReturnsTrue()
    {
        var rowId1 = AuthorityRowId.From(2);
        var rowId2 = AuthorityRowId.From(3);
        Assert.True(rowId1 != rowId2);
    }
}
