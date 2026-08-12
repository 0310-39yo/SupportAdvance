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
        var deptRowId = DepartmentRowId.From(1L);

        // Act
        var membership = DepartmentMembership.Create(membershipRowId, employeeRowId, deptRowId, IsPrimary.Primary());

        // Assert
        Assert.NotNull(membership);
        Assert.Equal(membershipRowId, membership.RowId);
        Assert.Equal(employeeRowId, membership.EmployeeRowId);
        Assert.Equal(deptRowId, membership.DepartmentRowId);
        Assert.True(membership.IsPrimary.Value);
    }

    [Fact]
    public void Create_WithSecondaryMembership_ReturnsSecondaryMembership()
    {
        // Arrange
        var membershipRowId = DepartmentMembershipRowId.From(2L);
        var employeeRowId = EmployeeRowId.From(100L);
        var deptRowId = DepartmentRowId.From(1L);

        // Act
        var membership = DepartmentMembership.Create(membershipRowId, employeeRowId, deptRowId, IsPrimary.Secondary());

        // Assert
        Assert.NotNull(membership);
        Assert.False(membership.IsPrimary.Value);
    }

    [Fact]
    public void Create_WithMultipleMemberships_ReturnsDistinctInstances()
    {
        // Arrange
        var membershipRowId1 = DepartmentMembershipRowId.From(3L);
        var employeeRowId1 = EmployeeRowId.From(100L);
        var membershipRowId2 = DepartmentMembershipRowId.From(4L);
        var employeeRowId2 = EmployeeRowId.From(101L);
        var deptRowId1 = DepartmentRowId.From(1L);
        var deptRowId2 = DepartmentRowId.From(2L);

        // Act
        var membership1 = DepartmentMembership.Create(membershipRowId1, employeeRowId1, deptRowId1, IsPrimary.Primary());
        var membership2 = DepartmentMembership.Create(membershipRowId2, employeeRowId2, deptRowId2, IsPrimary.Secondary());

        // Assert
        Assert.NotEqual(membership1.RowId, membership2.RowId);
        Assert.NotEqual(membership1.DepartmentRowId, membership2.DepartmentRowId);
    }

    #endregion

    #region グループ 2: 復元メソッド（Reconstruct）

    [Fact]
    public void Reconstruct_WithValidParameters_ReturnsValidMembership()
    {
        // Arrange
        var membershipRowId = DepartmentMembershipRowId.From(5L);
        var employeeRowId = EmployeeRowId.From(100L);
        var deptRowId = DepartmentRowId.From(1L);

        // Act
        var membership = DepartmentMembership.Reconstruct(membershipRowId, employeeRowId, deptRowId, IsPrimary.Primary(), EndOn.Unlimited);

        // Assert
        Assert.NotNull(membership);
        Assert.Equal(membershipRowId, membership.RowId);
        Assert.Equal(employeeRowId, membership.EmployeeRowId);
        Assert.Equal(deptRowId, membership.DepartmentRowId);
    }

    [Fact]
    public void Reconstruct_WithExpirationDate_ReturnsValidMembership()
    {
        // Arrange
        var membershipRowId = DepartmentMembershipRowId.From(6L);
        var employeeRowId = EmployeeRowId.From(100L);
        var deptRowId = DepartmentRowId.From(1L);
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));

        // Act
        var membership = DepartmentMembership.Reconstruct(membershipRowId, employeeRowId, deptRowId, IsPrimary.Primary(), EndOn.From(expirationDate));

        // Assert
        Assert.NotNull(membership);
        Assert.Equal(expirationDate, membership.EndOn.Value);
    }

    #endregion

    #region グループ 3: IsActive メソッド

    [Fact]
    public void IsActive_WithoutExpiration_ReturnsTrue()
    {
        // Arrange
        var membershipRowId = DepartmentMembershipRowId.From(7L);
        var employeeRowId = EmployeeRowId.From(100L);
        var deptRowId = DepartmentRowId.From(1L);
        var membership = DepartmentMembership.Create(membershipRowId, employeeRowId, deptRowId, IsPrimary.Primary());
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
        var deptRowId = DepartmentRowId.From(1L);
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));
        var membership = DepartmentMembership.Create(membershipRowId, employeeRowId, deptRowId, IsPrimary.Primary(), EndOn.From(expirationDate));
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
        var deptRowId = DepartmentRowId.From(1L);
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));
        var membership = DepartmentMembership.Create(membershipRowId, employeeRowId, deptRowId, IsPrimary.Primary(), EndOn.From(expirationDate));
        var checkDate = new LocalDateTime(new DateTime(2027, 1, 1, 0, 0, 0));

        // Act
        var isActive = membership.IsActive(checkDate);

        // Assert
        Assert.False(isActive);
    }

    #endregion
}
