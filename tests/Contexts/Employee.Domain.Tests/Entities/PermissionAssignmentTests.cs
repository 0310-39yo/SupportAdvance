namespace SupportAdvance.Contexts.Employee.Domain.Tests.Entities;

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using Xunit;

/// <summary>
/// PermissionAssignment Entity の単体テスト
/// </summary>
public class PermissionAssignmentTests
{
    private static LocalDateTime GetTestDate() => new(new DateTime(2026, 1, 1, 0, 0, 0));

    #region グループ 1: 生成メソッド（Create）

    [Fact]
    public void TestCreate01_WithExpirationDateReturnsValidPermissionAssignment()
    {
        // Arrange
        var assignmentRowId = PermissionAssignmentRowId.From(1L);
        var employeeRowId = EmployeeRowId.From(100L);
        var permissionCode = PermissionCode.From("Read");
        var effectiveDate = GetTestDate();
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));

        // Act
        var permissionAssignment = PermissionAssignment.Create(assignmentRowId, employeeRowId, permissionCode, EffectiveAt.From(effectiveDate), ExpirationOn.From(expirationDate));

        // Assert
        Assert.NotNull(permissionAssignment);
        Assert.Equal(assignmentRowId, permissionAssignment.RowId);
        Assert.Equal(employeeRowId, permissionAssignment.EmployeeRowId);
        Assert.Equal(permissionCode, permissionAssignment.PermissionCode);
        Assert.Equal(effectiveDate, permissionAssignment.EffectiveDate.Value);
        Assert.Equal(expirationDate, permissionAssignment.ExpirationDate.Value);
    }

    [Fact]
    public void TestCreate02_WithoutExpirationDateReturnsValidPermissionAssignment()
    {
        // Arrange
        var assignmentRowId = PermissionAssignmentRowId.From(2L);
        var employeeRowId = EmployeeRowId.From(100L);
        var permissionCode = PermissionCode.From("Write");
        var effectiveDate = GetTestDate();

        // Act
        var permissionAssignment = PermissionAssignment.Create(assignmentRowId, employeeRowId, permissionCode, EffectiveAt.From(effectiveDate));

        // Assert
        Assert.NotNull(permissionAssignment);
        Assert.Equal(assignmentRowId, permissionAssignment.RowId);
        Assert.Equal(employeeRowId, permissionAssignment.EmployeeRowId);
        Assert.Equal(permissionCode, permissionAssignment.PermissionCode);
        Assert.Equal(effectiveDate, permissionAssignment.EffectiveDate.Value);
        Assert.False(permissionAssignment.ExpirationDate.HasExpiration);  // Unlimited状態
    }

    #endregion

    #region グループ 2: 復元メソッド（Reconstruct）

    [Fact]
    public void TestReconstruct01_ReconstructFromDbValuesReturnsValidPermissionAssignment()
    {
        // Arrange
        var assignmentRowId = PermissionAssignmentRowId.From(100L);
        var employeeRowId = EmployeeRowId.From(12345L);
        var permissionCode = PermissionCode.From("Read");
        var effectiveDate = GetTestDate();
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));

        // Act
        var permissionAssignment = PermissionAssignment.Reconstruct(assignmentRowId, employeeRowId, permissionCode, EffectiveAt.From(effectiveDate), ExpirationOn.From(expirationDate));

        // Assert
        Assert.NotNull(permissionAssignment);
        Assert.Equal(assignmentRowId, permissionAssignment.RowId);
        Assert.Equal(employeeRowId, permissionAssignment.EmployeeRowId);
        Assert.Equal(permissionCode, permissionAssignment.PermissionCode);
        Assert.Equal(effectiveDate, permissionAssignment.EffectiveDate.Value);
    }

    #endregion

    #region グループ 3: IsActive メソッド

    [Fact]
    public void TestIsActive01_BeforeEffectiveDateReturnsFalse()
    {
        // Arrange
        var assignmentRowId = PermissionAssignmentRowId.From(3L);
        var employeeRowId = EmployeeRowId.From(1L);
        var permissionCode = PermissionCode.From("Read");
        var effectiveDate = GetTestDate();
        var permissionAssignment = PermissionAssignment.Create(assignmentRowId, employeeRowId, permissionCode, EffectiveAt.From(effectiveDate));
        var beforeDate = new LocalDateTime(new DateTime(2025, 12, 31, 23, 59, 59));

        // Act
        var isActive = permissionAssignment.IsActive(beforeDate);

        // Assert
        Assert.False(isActive);
    }

    [Fact]
    public void TestIsActive02_OnOrAfterEffectiveDateReturnsTrue()
    {
        // Arrange
        var assignmentRowId = PermissionAssignmentRowId.From(4L);
        var employeeRowId = EmployeeRowId.From(1L);
        var permissionCode = PermissionCode.From("Read");
        var effectiveDate = GetTestDate();
        var permissionAssignment = PermissionAssignment.Create(assignmentRowId, employeeRowId, permissionCode, EffectiveAt.From(effectiveDate));

        // Act
        var isActive = permissionAssignment.IsActive(effectiveDate);

        // Assert
        Assert.True(isActive);
    }

    [Fact]
    public void TestIsActive03_AfterExpirationDateReturnsFalse()
    {
        // Arrange
        var assignmentRowId = PermissionAssignmentRowId.From(5L);
        var employeeRowId = EmployeeRowId.From(1L);
        var permissionCode = PermissionCode.From("Read");
        var effectiveDate = GetTestDate();
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));
        var permissionAssignment = PermissionAssignment.Create(assignmentRowId, employeeRowId, permissionCode, EffectiveAt.From(effectiveDate), ExpirationOn.From(expirationDate));
        var afterDate = new LocalDateTime(new DateTime(2027, 1, 1, 0, 0, 0));

        // Act
        var isActive = permissionAssignment.IsActive(afterDate);

        // Assert
        Assert.False(isActive);
    }

    #endregion
}
