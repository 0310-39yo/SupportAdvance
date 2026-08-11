namespace SupportAdvance.Contexts.Employee.Domain.Tests.Entities;

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using Xunit;

/// <summary>
/// DepartmentMembership Entity の単体テスト
/// </summary>
public class DepartmentMembershipTests
{
    #region グループ 1: 生成メソッド（Create）

    [Fact]
    public void Create_WithValidParameters_ReturnsValidMembership()
    {
        // Arrange
        var membershipRowId = DepartmentMembershipRowId.From(1L);
        var employeeRowId = EmployeeRowId.From(100L);
        var deptCode = DepartmentCode.From("DEPT");

        // Act
        var membership = DepartmentMembership.Create(membershipRowId, employeeRowId, deptCode, true, null);

        // Assert
        Assert.NotNull(membership);
        Assert.Equal(membershipRowId, membership.RowId);
        Assert.Equal(employeeRowId, membership.EmployeeRowId);
        Assert.Equal(deptCode, membership.DepartmentCode);
        Assert.True(membership.IsPrimary);
    }

    [Fact]
    public void Create_WithSecondaryMembership_ReturnsSecondaryMembership()
    {
        // Arrange
        var membershipRowId = DepartmentMembershipRowId.From(2L);
        var employeeRowId = EmployeeRowId.From(100L);
        var deptCode = DepartmentCode.From("DEPT");

        // Act
        var membership = DepartmentMembership.Create(membershipRowId, employeeRowId, deptCode, false, null);

        // Assert
        Assert.NotNull(membership);
        Assert.False(membership.IsPrimary);
    }

    [Fact]
    public void Create_WithMultipleMemberships_ReturnsDistinctInstances()
    {
        // Arrange
        var membershipRowId1 = DepartmentMembershipRowId.From(3L);
        var employeeRowId1 = EmployeeRowId.From(100L);
        var membershipRowId2 = DepartmentMembershipRowId.From(4L);
        var employeeRowId2 = EmployeeRowId.From(101L);
        var deptCode1 = DepartmentCode.From("DEP1");
        var deptCode2 = DepartmentCode.From("DEP2");

        // Act
        var membership1 = DepartmentMembership.Create(membershipRowId1, employeeRowId1, deptCode1, true, null);
        var membership2 = DepartmentMembership.Create(membershipRowId2, employeeRowId2, deptCode2, false, null);

        // Assert
        Assert.NotEqual(membership1.RowId, membership2.RowId);
        Assert.NotEqual(membership1.DepartmentCode, membership2.DepartmentCode);
    }

    #endregion

    #region グループ 2: 復元メソッド（Reconstruct）

    [Fact]
    public void Reconstruct_WithValidParameters_ReturnsValidMembership()
    {
        // Arrange
        var membershipRowId = DepartmentMembershipRowId.From(5L);
        var employeeRowId = EmployeeRowId.From(100L);
        var deptCode = DepartmentCode.From("DEPT");

        // Act
        var membership = DepartmentMembership.Reconstruct(membershipRowId, employeeRowId, deptCode, true, null);

        // Assert
        Assert.NotNull(membership);
        Assert.Equal(membershipRowId, membership.RowId);
        Assert.Equal(employeeRowId, membership.EmployeeRowId);
        Assert.Equal(deptCode, membership.DepartmentCode);
    }

    [Fact]
    public void Reconstruct_WithExpirationDate_ReturnsValidMembership()
    {
        // Arrange
        var membershipRowId = DepartmentMembershipRowId.From(6L);
        var employeeRowId = EmployeeRowId.From(100L);
        var deptCode = DepartmentCode.From("DEPT");
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));

        // Act
        var membership = DepartmentMembership.Reconstruct(membershipRowId, employeeRowId, deptCode, true, expirationDate);

        // Assert
        Assert.NotNull(membership);
        Assert.Equal(expirationDate, membership.ExpirationDate);
    }

    #endregion

    #region グループ 3: IsActive メソッド

    [Fact]
    public void IsActive_WithoutExpiration_ReturnsTrue()
    {
        // Arrange
        var membershipRowId = DepartmentMembershipRowId.From(7L);
        var employeeRowId = EmployeeRowId.From(100L);
        var deptCode = DepartmentCode.From("DEPT");
        var membership = DepartmentMembership.Create(membershipRowId, employeeRowId, deptCode, true, null);
        var checkDate = new LocalDateTime(new DateTime(2026, 6, 1, 0, 0, 0));

        // Act
        var isActive = membership.IsActive(checkDate);

        // Assert
        Assert.True(isActive);
    }

    [Fact]
    public void IsActive_BeforeExpirationDate_ReturnsTrue()
    {
        // Arrange
        var membershipRowId = DepartmentMembershipRowId.From(8L);
        var employeeRowId = EmployeeRowId.From(100L);
        var deptCode = DepartmentCode.From("DEPT");
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));
        var membership = DepartmentMembership.Create(membershipRowId, employeeRowId, deptCode, true, expirationDate);
        var checkDate = new LocalDateTime(new DateTime(2026, 6, 1, 0, 0, 0));

        // Act
        var isActive = membership.IsActive(checkDate);

        // Assert
        Assert.True(isActive);
    }

    [Fact]
    public void IsActive_OnOrAfterExpirationDate_ReturnsFalse()
    {
        // Arrange
        var membershipRowId = DepartmentMembershipRowId.From(9L);
        var employeeRowId = EmployeeRowId.From(100L);
        var deptCode = DepartmentCode.From("DEPT");
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));
        var membership = DepartmentMembership.Create(membershipRowId, employeeRowId, deptCode, true, expirationDate);
        var checkDate = new LocalDateTime(new DateTime(2027, 1, 1, 0, 0, 0));

        // Act
        var isActive = membership.IsActive(checkDate);

        // Assert
        Assert.False(isActive);
    }

    #endregion
}
