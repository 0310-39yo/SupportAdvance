using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Xunit;

namespace SupportAdvance.Contexts.Employee.Domain.Tests.ValueObjects.Employee;

/// <summary>
/// DepartmentRowId ValueObject の単体テスト
/// 【責務】部署の DB 行 ID を管理
/// </summary>
public class DepartmentRowIdTests
{
    [Fact]
    public void Constructor_WithValidId_CreatesDepartmentRowId()
    {
        var departmentRowId = DepartmentRowId.From(10);
        Assert.NotNull(departmentRowId);
        Assert.Equal(10, departmentRowId.Value);
    }

    [Fact]
    public void Constructor_WithLargeId_CreatesDepartmentRowId()
    {
        var departmentRowId = DepartmentRowId.From(999999);
        Assert.NotNull(departmentRowId);
        Assert.Equal(999999, departmentRowId.Value);
    }

    [Fact]
    public void Equality_WorksCorrectly()
    {
        var rowId1 = DepartmentRowId.From(10);
        var rowId2 = DepartmentRowId.From(10);
        var rowId3 = DepartmentRowId.From(11);

        Assert.Equal(rowId1, rowId2);
        Assert.NotEqual(rowId1, rowId3);
    }
}
