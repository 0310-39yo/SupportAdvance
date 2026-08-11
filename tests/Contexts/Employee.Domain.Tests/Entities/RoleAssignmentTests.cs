namespace SupportAdvance.Contexts.Employee.Domain.Tests.Entities;

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using Xunit;

/// <summary>
/// RoleAssignment Entity の単体テスト
/// </summary>
public class RoleAssignmentTests
{
    private static LocalDateTime GetTestDate() => new(new DateTime(2026, 1, 1, 0, 0, 0));

    #region グループ 1: 生成メソッド（Create）

    [Fact]
    public void TestCreate01_WithExpirationDateReturnsValidRoleAssignment()
    {
        // Arrange
        var assignmentRowId = RoleAssignmentRowId.From(1L);
        var employeeRowId = EmployeeRowId.From(100L);
        var roleCode = RoleCode.From("Admin");
        var effectiveDate = GetTestDate();
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));

        // Act
        var roleAssignment = RoleAssignment.Create(assignmentRowId, employeeRowId, roleCode, effectiveDate, expirationDate);

        // Assert
        Assert.NotNull(roleAssignment);
        Assert.Equal(assignmentRowId, roleAssignment.RowId);
        Assert.Equal(employeeRowId, roleAssignment.EmployeeRowId);
        Assert.Equal(roleCode, roleAssignment.RoleCode);
        Assert.Equal(effectiveDate, roleAssignment.EffectiveDate);
        Assert.Equal(expirationDate, roleAssignment.ExpirationDate);
    }

    [Fact]
    public void TestCreate02_WithoutExpirationDateReturnsValidRoleAssignment()
    {
        // Arrange
        var assignmentRowId = RoleAssignmentRowId.From(2L);
        var employeeRowId = EmployeeRowId.From(100L);
        var roleCode = RoleCode.From("Manager");
        var effectiveDate = GetTestDate();

        // Act
        var roleAssignment = RoleAssignment.Create(assignmentRowId, employeeRowId, roleCode, effectiveDate);

        // Assert
        Assert.NotNull(roleAssignment);
        Assert.Equal(assignmentRowId, roleAssignment.RowId);
        Assert.Equal(employeeRowId, roleAssignment.EmployeeRowId);
        Assert.Equal(roleCode, roleAssignment.RoleCode);
        Assert.Equal(effectiveDate, roleAssignment.EffectiveDate);
        Assert.Null(roleAssignment.ExpirationDate);
    }

    #endregion

    #region グループ 2: 復元メソッド（Reconstruct）

    [Fact]
    public void TestReconstruct01_ReconstructFromDbValuesReturnsValidRoleAssignment()
    {
        // Arrange
        var assignmentRowId = RoleAssignmentRowId.From(100L);
        var employeeRowId = EmployeeRowId.From(12345L);
        var roleCode = RoleCode.From("Admin");
        var effectiveDate = GetTestDate();
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));

        // Act
        var roleAssignment = RoleAssignment.Reconstruct(assignmentRowId, employeeRowId, roleCode, effectiveDate, expirationDate);

        // Assert
        Assert.NotNull(roleAssignment);
        Assert.Equal(assignmentRowId, roleAssignment.RowId);
        Assert.Equal(employeeRowId, roleAssignment.EmployeeRowId);
        Assert.Equal(roleCode, roleAssignment.RoleCode);
        Assert.Equal(effectiveDate, roleAssignment.EffectiveDate);
    }

    #endregion

    #region グループ 3: IsActive メソッド

    [Fact]
    public void TestIsActive01_BeforeEffectiveDateReturnsFalse()
    {
        // Arrange
        var assignmentRowId = RoleAssignmentRowId.From(3L);
        var employeeRowId = EmployeeRowId.From(1L);
        var roleCode = RoleCode.From("Admin");
        var effectiveDate = GetTestDate();
        var roleAssignment = RoleAssignment.Create(assignmentRowId, employeeRowId, roleCode, effectiveDate);
        var beforeDate = new LocalDateTime(new DateTime(2025, 12, 31, 23, 59, 59));

        // Act
        var isActive = roleAssignment.IsActive(beforeDate);

        // Assert
        Assert.False(isActive);
    }

    [Fact]
    public void TestIsActive02_OnOrAfterEffectiveDateReturnsTrue()
    {
        // Arrange
        var assignmentRowId = RoleAssignmentRowId.From(4L);
        var employeeRowId = EmployeeRowId.From(1L);
        var roleCode = RoleCode.From("Admin");
        var effectiveDate = GetTestDate();
        var roleAssignment = RoleAssignment.Create(assignmentRowId, employeeRowId, roleCode, effectiveDate);

        // Act
        var isActive = roleAssignment.IsActive(effectiveDate);

        // Assert
        Assert.True(isActive);
    }

    [Fact]
    public void TestIsActive03_AfterExpirationDateReturnsFalse()
    {
        // Arrange
        var assignmentRowId = RoleAssignmentRowId.From(5L);
        var employeeRowId = EmployeeRowId.From(1L);
        var roleCode = RoleCode.From("Admin");
        var effectiveDate = GetTestDate();
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));
        var roleAssignment = RoleAssignment.Create(assignmentRowId, employeeRowId, roleCode, effectiveDate, expirationDate);
        var afterDate = new LocalDateTime(new DateTime(2027, 1, 1, 0, 0, 0));

        // Act
        var isActive = roleAssignment.IsActive(afterDate);

        // Assert
        Assert.False(isActive);
    }

    #endregion
}
