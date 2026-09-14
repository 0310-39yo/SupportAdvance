namespace SupportAdvance.Contexts.Department.Domain.Tests.ValueObjects;

using SupportAdvance.Contexts.Department.Domain.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

public class DepartmentRowIdTests
{
    [Fact]
    public void From_ValidValue_ReturnsInstance()
    {
        var rowId = DepartmentRowId.From(123);
        Assert.Equal(123, rowId.Value);
    }

    [Fact]
    public void From_InvalidValue_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DepartmentRowId.From(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => DepartmentRowId.From(-1));
    }

    [Fact]
    public void TryFrom_ValidValue_ReturnsTrue()
    {
        var result = DepartmentRowId.TryFrom(456, out var rowId);
        Assert.True(result);
        Assert.Equal(456, rowId.Value);
    }

    [Fact]
    public void TryFrom_InvalidValue_ReturnsFalse()
    {
        var result = DepartmentRowId.TryFrom(0, out _);
        Assert.False(result);
    }

    [Fact]
    public void TryFromDbValue_ValidValue_ReturnsTrue()
    {
        var result = DepartmentRowId.TryFromDbValue(789, out var rowId);
        Assert.True(result);
        Assert.Equal(789, rowId.Value);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var rowId1 = DepartmentRowId.From(100);
        var rowId2 = DepartmentRowId.From(100);
        Assert.Equal(rowId1, rowId2);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        var rowId1 = DepartmentRowId.From(100);
        var rowId2 = DepartmentRowId.From(200);
        Assert.NotEqual(rowId1, rowId2);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var rowId = DepartmentRowId.From(999);
        Assert.Equal("999", rowId.ToString());
    }
}
