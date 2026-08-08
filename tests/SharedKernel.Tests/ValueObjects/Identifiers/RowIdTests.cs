using SupportAdvance.SharedKernel.ValueObjects.Audit;
using Xunit;

namespace SupportAdvance.SharedKernel.Tests.ValueObjects.Identifiers;

public class RowIdTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(long.MaxValue)]
    public void From_ValidValue_ReturnsRowId(long value)
    {
        var rowId = RowId.From(value);

        Assert.NotNull(rowId);
        Assert.Equal(value, rowId.Value);
        Assert.True(rowId.IsSet);
    }

    [Fact]
    public void From_NegativeValue_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => RowId.From(-1));
    }

    [Fact]
    public void New_ReturnsUnassignedRowId()
    {
        var rowId = RowId.New();

        Assert.NotNull(rowId);
        Assert.Equal(0, rowId.Value);
        Assert.True(rowId.IsSet);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var rowId1 = RowId.From(42);
        var rowId2 = RowId.From(42);

        Assert.Equal(rowId1, rowId2);
        Assert.True(rowId1 == rowId2);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        var rowId1 = RowId.From(42);
        var rowId2 = RowId.From(100);

        Assert.NotEqual(rowId1, rowId2);
        Assert.True(rowId1 != rowId2);
    }

    [Fact]
    public void GetHashCode_SameValue_ReturnsSameHashCode()
    {
        var rowId1 = RowId.From(42);
        var rowId2 = RowId.From(42);

        Assert.Equal(rowId1.GetHashCode(), rowId2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsValueAsString()
    {
        var rowId = RowId.From(42);

        Assert.Equal("42", rowId.ToString());
    }

    [Fact]
    public void ToString_Unassigned_ReturnsZeroAsString()
    {
        var rowId = RowId.New();

        Assert.Equal("0", rowId.ToString());
    }
}
