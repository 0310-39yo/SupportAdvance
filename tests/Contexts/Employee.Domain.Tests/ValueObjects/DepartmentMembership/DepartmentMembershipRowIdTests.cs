using SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;
using Xunit;

namespace SupportAdvance.Contexts.Employee.Domain.Tests.ValueObjects.DepartmentMembership;

/// <summary>
/// DepartmentMembershipRowId ValueObject の単体テスト
/// 【責務】部署配属レコードの DB 行 ID を管理
/// </summary>
public class DepartmentMembershipRowIdTests
{
    [Fact]
    public void Constructor_WithValidId_CreatesDepartmentMembershipRowId()
    {
        var rowId = DepartmentMembershipRowId.From(100);
        Assert.NotNull(rowId);
        Assert.Equal(100, rowId.Value);
    }

    [Fact]
    public void Constructor_WithLargeId_CreatesDepartmentMembershipRowId()
    {
        var rowId = DepartmentMembershipRowId.From(9999);
        Assert.NotNull(rowId);
        Assert.Equal(9999, rowId.Value);
    }

    [Fact]
    public void Equality_WorksCorrectly()
    {
        var rowId1 = DepartmentMembershipRowId.From(100);
        var rowId2 = DepartmentMembershipRowId.From(100);
        var rowId3 = DepartmentMembershipRowId.From(101);

        Assert.Equal(rowId1, rowId2);
        Assert.NotEqual(rowId1, rowId3);
    }
}
