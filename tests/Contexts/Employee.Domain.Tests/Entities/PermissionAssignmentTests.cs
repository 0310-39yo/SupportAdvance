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
    #region グループ 1: 生成メソッド（Create）

    [Fact]
    public void TestCreate01_WithExpirationDateReturnsValidPermissionAssignment()
    {
        // Arrange
        var permissionCode = PermissionCode.From("read");
        var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));

        // Act
        var permissionAssignment = PermissionAssignment.Create(permissionCode, effectiveDate, expirationDate);

        // Assert
        Assert.NotNull(permissionAssignment);
        Assert.Equal(permissionCode, permissionAssignment.PermissionCode);
        Assert.Equal(effectiveDate, permissionAssignment.EffectiveDate);
        Assert.Equal(expirationDate, permissionAssignment.ExpirationDate);
        Assert.NotEqual(Guid.Empty, permissionAssignment.Id.Value);
    }

    [Fact]
    public void TestCreate02_WithoutExpirationDateReturnsValidPermissionAssignment()
    {
        // Arrange
        var permissionCode = PermissionCode.From("write");
        var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));

        // Act
        var permissionAssignment = PermissionAssignment.Create(permissionCode, effectiveDate);

        // Assert
        Assert.NotNull(permissionAssignment);
        Assert.Equal(permissionCode, permissionAssignment.PermissionCode);
        Assert.Equal(effectiveDate, permissionAssignment.EffectiveDate);
        Assert.Null(permissionAssignment.ExpirationDate);
    }

    [Fact]
    public void TestCreate03_MultipleCreatesGenerateDifferentIds()
    {
        // Arrange & Act
        var perm1 = PermissionAssignment.Create(
            PermissionCode.From("read"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        var perm2 = PermissionAssignment.Create(
            PermissionCode.From("write"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        // Assert
        Assert.NotEqual(perm1.Id, perm2.Id);
    }

    #endregion

    #region グループ 2: 復元メソッド（Reconstruct）

    [Fact]
    public void TestReconstruct01_WithExpirationDateReturnsValidPermissionAssignment()
    {
        // Arrange
        var id = PermissionAssignmentId.NewId();
        var permissionCode = PermissionCode.From("read");
        var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));

        // Act
        var permissionAssignment = PermissionAssignment.Reconstruct(id, permissionCode, effectiveDate, expirationDate);

        // Assert
        Assert.NotNull(permissionAssignment);
        Assert.Equal(id, permissionAssignment.Id);
        Assert.Equal(permissionCode, permissionAssignment.PermissionCode);
        Assert.Equal(effectiveDate, permissionAssignment.EffectiveDate);
        Assert.Equal(expirationDate, permissionAssignment.ExpirationDate);
    }

    [Fact]
    public void TestReconstruct02_WithoutExpirationDateReturnsValidPermissionAssignment()
    {
        // Arrange
        var id = PermissionAssignmentId.NewId();
        var permissionCode = PermissionCode.From("write");
        var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));

        // Act
        var permissionAssignment = PermissionAssignment.Reconstruct(id, permissionCode, effectiveDate);

        // Assert
        Assert.Equal(id, permissionAssignment.Id);
        Assert.Null(permissionAssignment.ExpirationDate);
    }

    #endregion

    #region グループ 3: ビジネスロジック（IsActive）

    [Fact]
    public void TestIsActive01_WithinEffectivePeriodReturnsTrue()
    {
        // Arrange
        var permissionAssignment = PermissionAssignment.Create(
            PermissionCode.From("read"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)),
            new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59)));

        var checkDate = new LocalDateTime(new DateTime(2026, 6, 15, 12, 0, 0));

        // Act
        var isActive = permissionAssignment.IsActive(checkDate);

        // Assert
        Assert.True(isActive);
    }

    [Fact]
    public void TestIsActive02_BeforeEffectiveDateReturnsFalse()
    {
        // Arrange
        var permissionAssignment = PermissionAssignment.Create(
            PermissionCode.From("read"),
            new LocalDateTime(new DateTime(2026, 6, 1, 0, 0, 0)),
            new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59)));

        var checkDate = new LocalDateTime(new DateTime(2026, 5, 31, 23, 59, 59));

        // Act
        var isActive = permissionAssignment.IsActive(checkDate);

        // Assert
        Assert.False(isActive);
    }

    [Fact]
    public void TestIsActive03_AfterExpirationDateReturnsFalse()
    {
        // Arrange
        var permissionAssignment = PermissionAssignment.Create(
            PermissionCode.From("read"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)),
            new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59)));

        var checkDate = new LocalDateTime(new DateTime(2027, 1, 1, 0, 0, 0));

        // Act
        var isActive = permissionAssignment.IsActive(checkDate);

        // Assert
        Assert.False(isActive);
    }

    [Fact]
    public void TestIsActive04_WithoutExpirationDateAlwaysReturnsTrueAfterEffectiveDate()
    {
        // Arrange
        var permissionAssignment = PermissionAssignment.Create(
            PermissionCode.From("read"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        var checkDate = new LocalDateTime(new DateTime(2099, 12, 31, 23, 59, 59));

        // Act
        var isActive = permissionAssignment.IsActive(checkDate);

        // Assert
        Assert.True(isActive);
    }

    [Fact]
    public void TestIsActive05_OnEffectiveDateReturnsTrue()
    {
        // Arrange
        var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
        var permissionAssignment = PermissionAssignment.Create(
            PermissionCode.From("read"),
            effectiveDate,
            new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59)));

        // Act
        var isActive = permissionAssignment.IsActive(effectiveDate);

        // Assert
        Assert.True(isActive);
    }

    #endregion

    #region グループ 4: プロパティアクセス

    [Fact]
    public void TestProperties01_PropertiesAreReadOnly()
    {
        // Arrange
        var permissionAssignment = PermissionAssignment.Create(
            PermissionCode.From("read"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        // Act & Assert
        // 以下はコンパイルエラー（CS0200: Property cannot be assigned to）
        // permissionAssignment.PermissionCode = newCode;
        // permissionAssignment.EffectiveDate = newDate;

        // 読み取りのみ可能
        Assert.NotNull(permissionAssignment.PermissionCode);
        Assert.NotEqual(DateTime.MinValue, permissionAssignment.EffectiveDate.Value);
    }

    [Fact]
    public void TestProperties02_PropertyImmutability()
    {
        // Arrange
        var permissionAssignment = PermissionAssignment.Create(
            PermissionCode.From("read"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        // Act
        var code1 = permissionAssignment.PermissionCode;
        var code2 = permissionAssignment.PermissionCode;

        // Assert
        Assert.Same(code1, code2);  // 同じインスタンス
    }

    #endregion

    #region グループ 5: 等価性（Equality）

    [Fact]
    public void TestEquality01_SameIdAreEqual()
    {
        // Arrange
        var id = PermissionAssignmentId.NewId();
        var perm1 = PermissionAssignment.Reconstruct(
            id,
            PermissionCode.From("read"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        var perm2 = PermissionAssignment.Reconstruct(
            id,
            PermissionCode.From("write"),
            new LocalDateTime(new DateTime(2026, 6, 1, 0, 0, 0)));

        // Assert
        Assert.Equal(perm1, perm2);  // Entity<TId> は Id で比較
    }

    [Fact]
    public void TestEquality02_DifferentIdAreNotEqual()
    {
        // Arrange
        var perm1 = PermissionAssignment.Create(
            PermissionCode.From("read"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        var perm2 = PermissionAssignment.Create(
            PermissionCode.From("read"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        // Assert
        Assert.NotEqual(perm1, perm2);
    }

    [Fact]
    public void TestEquality03_HashCodesAreEqual()
    {
        // Arrange
        var id = PermissionAssignmentId.NewId();
        var perm1 = PermissionAssignment.Reconstruct(
            id,
            PermissionCode.From("read"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        var perm2 = PermissionAssignment.Reconstruct(
            id,
            PermissionCode.From("read"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        // Assert
        Assert.Equal(perm1.GetHashCode(), perm2.GetHashCode());
    }

    [Fact]
    public void TestEquality04_CanBeUsedAsDictionaryKey()
    {
        // Arrange
        var id = PermissionAssignmentId.NewId();
        var perm1 = PermissionAssignment.Reconstruct(
            id,
            PermissionCode.From("read"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        var perm2 = PermissionAssignment.Reconstruct(
            id,
            PermissionCode.From("read"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        var dict = new Dictionary<PermissionAssignment, string>();

        // Act
        dict.Add(perm1, "Permission1");
        dict[perm2] = "Permission2";  // 同じ ID なので上書き

        // Assert
        Assert.Single(dict);
        Assert.Equal("Permission2", dict[perm1]);
    }

    [Fact]
    public void TestEquality05_EqualsNullReturnsFalse()
    {
        // Arrange
        var permissionAssignment = PermissionAssignment.Create(
            PermissionCode.From("read"),
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        // Act & Assert
        Assert.False(permissionAssignment.Equals(null));
    }

    #endregion

    #region グループ 6: 統合テスト

    [Fact]
    public void TestIntegration01_AllPropertiesAreCoherent()
    {
        // Arrange
        var permissionCode = PermissionCode.From("read");
        var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));

        // Act
        var permissionAssignment = PermissionAssignment.Create(permissionCode, effectiveDate, expirationDate);

        // Assert - すべてのプロパティが有効
        Assert.NotEqual(Guid.Empty, permissionAssignment.Id.Value);
        Assert.Equal(permissionCode, permissionAssignment.PermissionCode);
        Assert.Equal(effectiveDate, permissionAssignment.EffectiveDate);
        Assert.Equal(expirationDate, permissionAssignment.ExpirationDate);
        Assert.True(permissionAssignment.IsActive(new LocalDateTime(new DateTime(2026, 6, 15, 0, 0, 0))));
    }

    [Fact]
    public void TestIntegration02_CreateAndReconstructAreConsistent()
    {
        // Arrange
        var permissionCode = PermissionCode.From("write");
        var effectiveDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));

        // Act
        var created = PermissionAssignment.Create(permissionCode, effectiveDate);
        var reconstructed = PermissionAssignment.Reconstruct(created.Id, permissionCode, effectiveDate);

        // Assert
        Assert.Equal(created.Id, reconstructed.Id);
        Assert.Equal(created.PermissionCode, reconstructed.PermissionCode);
        Assert.Equal(created.EffectiveDate, reconstructed.EffectiveDate);
    }

    #endregion
}
