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
    #region グループ 1: 生成メソッド（Create）

    [Fact]
    public void TestCreate01_WithExpirationDateReturnsValidRoleAssignment()
    {
        // Arrange
        var roleCode = RoleCode.From("Admin");
        var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));

        // Act
        var roleAssignment = RoleAssignment.Create(roleCode, effectiveDate, expirationDate);

        // Assert
        Assert.NotNull(roleAssignment);
        Assert.Equal(roleCode, roleAssignment.RoleCode);
        Assert.Equal(effectiveDate, roleAssignment.EffectiveDate);
        Assert.Equal(expirationDate, roleAssignment.ExpirationDate);
        Assert.NotEqual(Guid.Empty, roleAssignment.Id.Value);
    }

    [Fact]
    public void TestCreate02_WithoutExpirationDateReturnsValidRoleAssignment()
    {
        // Arrange
        var roleCode = RoleCode.From("Manager");
        var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));

        // Act
        var roleAssignment = RoleAssignment.Create(roleCode, effectiveDate);

        // Assert
        Assert.NotNull(roleAssignment);
        Assert.Equal(roleCode, roleAssignment.RoleCode);
        Assert.Equal(effectiveDate, roleAssignment.EffectiveDate);
        Assert.Null(roleAssignment.ExpirationDate);
    }

    [Fact]
    public void TestCreate03_MultipleCreatesGenerateDifferentIds()
    {
        // Arrange & Act
        var role1 = RoleAssignment.Create(
            RoleCode.From("Admin"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        var role2 = RoleAssignment.Create(
            RoleCode.From("Manager"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        // Assert
        Assert.NotEqual(role1.Id, role2.Id);
    }

    #endregion

    #region グループ 2: 復元メソッド（Reconstruct）

    [Fact]
    public void TestReconstruct01_WithExpirationDateReturnsValidRoleAssignment()
    {
        // Arrange
        var id = RoleAssignmentId.NewId();
        var roleCode = RoleCode.From("Admin");
        var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));

        // Act
        var roleAssignment = RoleAssignment.Reconstruct(id, roleCode, effectiveDate, expirationDate);

        // Assert
        Assert.NotNull(roleAssignment);
        Assert.Equal(id, roleAssignment.Id);
        Assert.Equal(roleCode, roleAssignment.RoleCode);
        Assert.Equal(effectiveDate, roleAssignment.EffectiveDate);
        Assert.Equal(expirationDate, roleAssignment.ExpirationDate);
    }

    [Fact]
    public void TestReconstruct02_WithoutExpirationDateReturnsValidRoleAssignment()
    {
        // Arrange
        var id = RoleAssignmentId.NewId();
        var roleCode = RoleCode.From("Manager");
        var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));

        // Act
        var roleAssignment = RoleAssignment.Reconstruct(id, roleCode, effectiveDate);

        // Assert
        Assert.Equal(id, roleAssignment.Id);
        Assert.Null(roleAssignment.ExpirationDate);
    }

    #endregion

    #region グループ 3: ビジネスロジック（IsActive）

    [Fact]
    public void TestIsActive01_WithinEffectivePeriodReturnsTrue()
    {
        // Arrange
        var roleAssignment = RoleAssignment.Create(
            RoleCode.From("Admin"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)),
            new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59)));

        var checkDate = new LocalDateTime(new DateTime(2026, 6, 15, 12, 0, 0));

        // Act
        var isActive = roleAssignment.IsActive(checkDate);

        // Assert
        Assert.True(isActive);
    }

    [Fact]
    public void TestIsActive02_BeforeEffectiveDateReturnsFalse()
    {
        // Arrange
        var roleAssignment = RoleAssignment.Create(
            RoleCode.From("Admin"),
            new LocalDateTime(new DateTime(2026, 6, 1, 0, 0, 0)),
            new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59)));

        var checkDate = new LocalDateTime(new DateTime(2026, 5, 31, 23, 59, 59));

        // Act
        var isActive = roleAssignment.IsActive(checkDate);

        // Assert
        Assert.False(isActive);
    }

    [Fact]
    public void TestIsActive03_AfterExpirationDateReturnsFalse()
    {
        // Arrange
        var roleAssignment = RoleAssignment.Create(
            RoleCode.From("Admin"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)),
            new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59)));

        var checkDate = new LocalDateTime(new DateTime(2027, 1, 1, 0, 0, 0));

        // Act
        var isActive = roleAssignment.IsActive(checkDate);

        // Assert
        Assert.False(isActive);
    }

    [Fact]
    public void TestIsActive04_WithoutExpirationDateAlwaysReturnsTrueAfterEffectiveDate()
    {
        // Arrange
        var roleAssignment = RoleAssignment.Create(
            RoleCode.From("Admin"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        var checkDate = new LocalDateTime(new DateTime(2099, 12, 31, 23, 59, 59));

        // Act
        var isActive = roleAssignment.IsActive(checkDate);

        // Assert
        Assert.True(isActive);
    }

    [Fact]
    public void TestIsActive05_OnEffectiveDateReturnsTrue()
    {
        // Arrange
        var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
        var roleAssignment = RoleAssignment.Create(
            RoleCode.From("Admin"),
            effectiveDate,
            new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59)));

        // Act
        var isActive = roleAssignment.IsActive(effectiveDate);

        // Assert
        Assert.True(isActive);
    }

    #endregion

    #region グループ 4: プロパティアクセス

    [Fact]
    public void TestProperties01_PropertiesAreReadOnly()
    {
        // Arrange
        var roleAssignment = RoleAssignment.Create(
            RoleCode.From("Admin"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        // Act & Assert
        // 以下はコンパイルエラー（CS0200: Property cannot be assigned to）
        // roleAssignment.RoleCode = newCode;
        // roleAssignment.EffectiveDate = newDate;

        // 読み取りのみ可能
        Assert.NotNull(roleAssignment.RoleCode);
        Assert.NotEqual(DateTime.MinValue, roleAssignment.EffectiveDate.Value);
    }

    [Fact]
    public void TestProperties02_PropertyImmutability()
    {
        // Arrange
        var roleAssignment = RoleAssignment.Create(
            RoleCode.From("Admin"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        // Act
        var code1 = roleAssignment.RoleCode;
        var code2 = roleAssignment.RoleCode;

        // Assert
        Assert.Same(code1, code2);  // 同じインスタンス
    }

    #endregion

    #region グループ 5: 等価性（Equality）

    [Fact]
    public void TestEquality01_SameIdAreEqual()
    {
        // Arrange
        var id = RoleAssignmentId.NewId();
        var role1 = RoleAssignment.Reconstruct(
            id,
            RoleCode.From("Admin"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        var role2 = RoleAssignment.Reconstruct(
            id,
            RoleCode.From("Manager"),
            new LocalDateTime(new DateTime(2026, 6, 1, 0, 0, 0)));

        // Assert
        Assert.Equal(role1, role2);  // Entity<TId> は Id で比較
    }

    [Fact]
    public void TestEquality02_DifferentIdAreNotEqual()
    {
        // Arrange
        var role1 = RoleAssignment.Create(
            RoleCode.From("Admin"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        var role2 = RoleAssignment.Create(
            RoleCode.From("Admin"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        // Assert
        Assert.NotEqual(role1, role2);
    }

    [Fact]
    public void TestEquality03_HashCodesAreEqual()
    {
        // Arrange
        var id = RoleAssignmentId.NewId();
        var role1 = RoleAssignment.Reconstruct(
            id,
            RoleCode.From("Admin"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        var role2 = RoleAssignment.Reconstruct(
            id,
            RoleCode.From("Admin"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        // Assert
        Assert.Equal(role1.GetHashCode(), role2.GetHashCode());
    }

    [Fact]
    public void TestEquality04_CanBeUsedAsDictionaryKey()
    {
        // Arrange
        var id = RoleAssignmentId.NewId();
        var role1 = RoleAssignment.Reconstruct(
            id,
            RoleCode.From("Admin"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        var role2 = RoleAssignment.Reconstruct(
            id,
            RoleCode.From("Admin"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        var dict = new Dictionary<RoleAssignment, string>();

        // Act
        dict.Add(role1, "Role1");
        dict[role2] = "Role2";  // 同じ ID なので上書き

        // Assert
        Assert.Single(dict);
        Assert.Equal("Role2", dict[role1]);
    }

    [Fact]
    public void TestEquality05_EqualsNullReturnsFalse()
    {
        // Arrange
        var roleAssignment = RoleAssignment.Create(
            RoleCode.From("Admin"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        // Act & Assert
        Assert.False(roleAssignment.Equals(null));
    }

    #endregion

    #region グループ 6: 統合テスト

    [Fact]
    public void TestIntegration01_AllPropertiesAreCoherent()
    {
        // Arrange
        var roleCode = RoleCode.From("Admin");
        var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));

        // Act
        var roleAssignment = RoleAssignment.Create(roleCode, effectiveDate, expirationDate);

        // Assert - すべてのプロパティが有効
        Assert.NotEqual(Guid.Empty, roleAssignment.Id.Value);
        Assert.Equal(roleCode, roleAssignment.RoleCode);
        Assert.Equal(effectiveDate, roleAssignment.EffectiveDate);
        Assert.Equal(expirationDate, roleAssignment.ExpirationDate);
        Assert.True(roleAssignment.IsActive(new LocalDateTime(new DateTime(2026, 6, 15, 0, 0, 0))));
    }

    [Fact]
    public void TestIntegration02_CreateAndReconstructAreConsistent()
    {
        // Arrange
        var roleCode = RoleCode.From("Manager");
        var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));

        // Act
        var created = RoleAssignment.Create(roleCode, effectiveDate);
        var reconstructed = RoleAssignment.Reconstruct(created.Id, roleCode, effectiveDate);

        // Assert
        Assert.Equal(created.Id, reconstructed.Id);
        Assert.Equal(created.RoleCode, reconstructed.RoleCode);
        Assert.Equal(created.EffectiveDate, reconstructed.EffectiveDate);
    }

    #endregion
}
