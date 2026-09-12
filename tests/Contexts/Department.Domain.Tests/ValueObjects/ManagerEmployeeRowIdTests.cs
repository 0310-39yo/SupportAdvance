namespace SupportAdvance.Contexts.Department.Domain.Tests.ValueObjects;

using SupportAdvance.Contexts.Department.Domain.ValueObjects;

public class ManagerEmployeeRowIdTests
{
    [Fact]
    public void Unset_ReturnsInstance()
    {
        var managerId = ManagerEmployeeRowId.Unset();
        Assert.False(managerId.HasManager);
    }

    [Fact]
    public void From_ValidValue_ReturnsInstance()
    {
        var managerId = ManagerEmployeeRowId.From(200);
        Assert.True(managerId.HasManager);
        Assert.Equal(200, managerId.Value);
    }

    [Fact]
    public void From_InvalidValue_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ManagerEmployeeRowId.From(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => ManagerEmployeeRowId.From(-1));
    }

    [Fact]
    public void TryFrom_Null_ReturnsUnset()
    {
        var result = ManagerEmployeeRowId.TryFrom(null, out var managerId);
        Assert.True(result);
        Assert.False(managerId.HasManager);
    }

    [Fact]
    public void TryFrom_ValidValue_ReturnsInstance()
    {
        var result = ManagerEmployeeRowId.TryFrom(75, out var managerId);
        Assert.True(result);
        Assert.Equal(75, managerId.Value);
    }

    [Fact]
    public void TryFromDbValue_Null_ReturnsUnset()
    {
        var result = ManagerEmployeeRowId.TryFromDbValue(null, out var managerId);
        Assert.True(result);
        Assert.False(managerId.HasManager);
    }
}
