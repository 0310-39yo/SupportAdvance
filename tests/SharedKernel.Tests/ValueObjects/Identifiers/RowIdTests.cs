using SupportAdvance.SharedKernel.Tests.ValueObjects.Identifiers.Fixtures;

namespace SupportAdvance.Tests.SharedKernel.Tests.ValueObjects.Identifiers;

/// <summary>
/// RowId abstract base class の型安全性テスト
/// TestPersonRowId（テスト用具体的な実装）を使用してテスト
/// </summary>
public class RowIdTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(long.MaxValue)]
    public void From_ValidValue_ReturnsRowId(long value)
    {
        var rowId = TestPersonRowId.From(value);

        Assert.NotNull(rowId);
        Assert.Equal(value, rowId.Value);
        Assert.True(rowId.IsSet);
    }

    [Fact]
    public void From_InvalidValue_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TestPersonRowId.From(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => TestPersonRowId.From(-1));
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var rowId1 = TestPersonRowId.From(42);
        var rowId2 = TestPersonRowId.From(42);

        Assert.Equal(rowId1, rowId2);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        var rowId1 = TestPersonRowId.From(42);
        var rowId2 = TestPersonRowId.From(100);

        Assert.NotEqual(rowId1, rowId2);
    }

    [Fact]
    public void GetHashCode_SameValue_ReturnsSameHashCode()
    {
        var rowId1 = TestPersonRowId.From(42);
        var rowId2 = TestPersonRowId.From(42);

        Assert.Equal(rowId1.GetHashCode(), rowId2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsValueAsString()
    {
        var rowId = TestPersonRowId.From(42);

        Assert.Equal("42", rowId.ToString());
    }
}

