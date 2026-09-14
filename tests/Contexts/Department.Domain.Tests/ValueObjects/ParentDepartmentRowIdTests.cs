namespace SupportAdvance.Contexts.Department.Domain.Tests.ValueObjects;

using SupportAdvance.Contexts.Department.Domain.ValueObjects;

public class ParentDepartmentRowIdTests
{
    [Fact]
    public void Unset_ReturnsInstance()
    {
        var parentId = ParentDepartmentRowId.Unset();
        Assert.False(parentId.HasParent);
    }

    [Fact]
    public void From_ValidValue_ReturnsInstance()
    {
        var parentId = ParentDepartmentRowId.From(100);
        Assert.True(parentId.HasParent);
        Assert.Equal(100, parentId.Value);
    }

    [Fact]
    public void From_InvalidValue_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ParentDepartmentRowId.From(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => ParentDepartmentRowId.From(-1));
    }

    [Fact]
    public void TryFrom_Null_ReturnsUnset()
    {
        var result = ParentDepartmentRowId.TryFrom(null, out var parentId);
        Assert.True(result);
        Assert.False(parentId.HasParent);
    }

    [Fact]
    public void TryFrom_ValidValue_ReturnsInstance()
    {
        var result = ParentDepartmentRowId.TryFrom(50, out var parentId);
        Assert.True(result);
        Assert.Equal(50, parentId.Value);
    }

    [Fact]
    public void TryFromDbValue_Null_ReturnsUnset()
    {
        var result = ParentDepartmentRowId.TryFromDbValue(null, out var parentId);
        Assert.True(result);
        Assert.False(parentId.HasParent);
    }
}
