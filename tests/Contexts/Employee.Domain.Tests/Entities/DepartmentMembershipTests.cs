namespace SupportAdvance.Contexts.Employee.Domain.Tests.Entities;

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Identifiers;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
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
        var deptCode = DepartmentCode.From("DEPT");
        var startDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));

        // Act
        var membership = DepartmentMembership.Create(deptCode, isPrimary: true, startDate);

        // Assert
        Assert.NotNull(membership);
        Assert.Equal(deptCode, membership.DepartmentCode);
        Assert.True(membership.IsPrimary);
    }

    [Fact]
    public void Create_WithSecondaryMembership_ReturnsSecondaryMembership()
    {
        // Arrange
        var deptCode = DepartmentCode.From("DEPT");
        var startDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));

        // Act
        var membership = DepartmentMembership.Create(deptCode, isPrimary: false, startDate);

        // Assert
        Assert.NotNull(membership);
        Assert.False(membership.IsPrimary);
    }

    [Fact]
    public void Create_WithMultipleMemberships_ReturnsDistinctInstances()
    {
        // Arrange
        var deptCode1 = DepartmentCode.From("DEP1");
        var deptCode2 = DepartmentCode.From("DEP2");
        var startDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));

        // Act
        var membership1 = DepartmentMembership.Create(deptCode1, isPrimary: true, startDate);
        var membership2 = DepartmentMembership.Create(deptCode2, isPrimary: false, startDate);

        // Assert
        Assert.NotEqual(membership1.Id, membership2.Id);
        Assert.NotEqual(membership1.DepartmentCode, membership2.DepartmentCode);
    }

    #endregion

    #region グループ 2: 復元メソッド（Reconstruct）

    [Fact]
    public void Reconstruct_WithValidParameters_ReturnsValidMembership()
    {
        // Arrange
        var id = DepartmentMembershipId.NewId();
        var deptCode = DepartmentCode.From("DEPT");
        var startDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));

        // Act
        var membership = DepartmentMembership.Reconstruct(id, deptCode, isPrimary: true, startDate);

        // Assert
        Assert.NotNull(membership);
        Assert.Equal(id, membership.Id);
        Assert.Equal(deptCode, membership.DepartmentCode);
    }

    [Fact]
    public void Reconstruct_WithExpirationDate_ReturnsValidMembership()
    {
        // Arrange
        var id = DepartmentMembershipId.NewId();
        var deptCode = DepartmentCode.From("DEPT");
        var startDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));

        // Act
        var membership = DepartmentMembership.Reconstruct(id, deptCode, isPrimary: true, startDate, expirationDate);

        // Assert
        Assert.NotNull(membership);
        Assert.Equal(expirationDate, membership.ExpirationDate);
    }

    #endregion

    #region グループ 3: プロパティアクセス

    [Fact]
    public void DepartmentCode_Property_ReturnsCorrectValue()
    {
        // Arrange
        var deptCode = DepartmentCode.From("DEPT");
        var startDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
        var membership = DepartmentMembership.Create(deptCode, isPrimary: true, startDate);

        // Act
        var result = membership.DepartmentCode;

        // Assert
        Assert.Equal(deptCode, result);
    }

    [Fact]
    public void IsPrimary_Property_ReturnsCorrectValue()
    {
        // Arrange
        var deptCode = DepartmentCode.From("DEPT");
        var startDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
        var membership = DepartmentMembership.Create(deptCode, isPrimary: true, startDate);

        // Act
        var result = membership.IsPrimary;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ExpirationDate_Property_ReturnsCorrectValue()
    {
        // Arrange
        var deptCode = DepartmentCode.From("DEPT");
        var startDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));
        var membership = DepartmentMembership.Create(deptCode, isPrimary: true, startDate, expirationDate);

        // Act
        var result = membership.ExpirationDate;

        // Assert
        Assert.Equal(expirationDate, result);
    }

    [Fact]
    public void Properties_AreReadOnly()
    {
        // Arrange
        var membership = DepartmentMembership.Create(
            DepartmentCode.From("DEPT"), isPrimary: true, new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        // Act & Assert
        // 以下はコンパイルエラーになる（CS0200: Property cannot be assigned to）
        // membership.DepartmentCode = newCode;
        // membership.IsPrimary = false;

        // 読み取りのみ可能
        Assert.NotNull(membership.DepartmentCode);
    }

    #endregion

    #region グループ 4: ビジネスロジック

    [Fact]
    public void IsActive_WithoutExpiration_ReturnsTrue()
    {
        // Arrange
        var deptCode = DepartmentCode.From("DEPT");
        var startDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
        var membership = DepartmentMembership.Create(deptCode, isPrimary: true, startDate);
        var now = new LocalDateTime(new DateTime(2026, 6, 15, 12, 0, 0));

        // Act
        var result = membership.IsActive(now);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsActive_BeforeExpiration_ReturnsTrue()
    {
        // Arrange
        var deptCode = DepartmentCode.From("DEPT");
        var startDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));
        var membership = DepartmentMembership.Create(deptCode, isPrimary: true, startDate, expirationDate);
        var now = new LocalDateTime(new DateTime(2026, 6, 15, 12, 0, 0));

        // Act
        var result = membership.IsActive(now);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsActive_OnExpirationDate_ReturnsFalse()
    {
        // Arrange
        var deptCode = DepartmentCode.From("DEPT");
        var startDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));
        var membership = DepartmentMembership.Create(deptCode, isPrimary: true, startDate, expirationDate);
        var now = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));

        // Act
        var result = membership.IsActive(now);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsActive_AfterExpiration_ReturnsFalse()
    {
        // Arrange
        var deptCode = DepartmentCode.From("DEPT");
        var startDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));
        var membership = DepartmentMembership.Create(deptCode, isPrimary: true, startDate, expirationDate);
        var now = new LocalDateTime(new DateTime(2027, 1, 1, 0, 0, 0));

        // Act
        var result = membership.IsActive(now);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region グループ 5: 等価性（Equality）

    [Fact]
    public void SameMembershipId_AreEqual()
    {
        // Arrange
        var id = DepartmentMembershipId.NewId();
        var membership1 = DepartmentMembership.Reconstruct(
            id,
            DepartmentCode.From("DEP1"),
            isPrimary: true,
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        var membership2 = DepartmentMembership.Reconstruct(
            id,
            DepartmentCode.From("DEP2"),  // 異なる部署
            isPrimary: false,  // 異なる主副
            new LocalDateTime(new DateTime(2026, 6, 1, 0, 0, 0)));

        // Assert
        Assert.Equal(membership1, membership2);  // Entity<TId> は Id で比較
    }

    [Fact]
    public void DifferentMembershipId_AreNotEqual()
    {
        // Arrange
        var membership1 = DepartmentMembership.Create(
            DepartmentCode.From("DEP1"),
            isPrimary: true,
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        var membership2 = DepartmentMembership.Create(
            DepartmentCode.From("DEP1"),
            isPrimary: true,
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        // Assert
        Assert.NotEqual(membership1, membership2);
    }

    [Fact]
    public void HashCodes_AreEqual_ForSameMembershipId()
    {
        // Arrange
        var id = DepartmentMembershipId.NewId();
        var membership1 = DepartmentMembership.Reconstruct(
            id,
            DepartmentCode.From("DEPT"),
            isPrimary: true,
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        var membership2 = DepartmentMembership.Reconstruct(
            id,
            DepartmentCode.From("DEPT"),
            isPrimary: true,
            new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0)));

        // Assert
        Assert.Equal(membership1.GetHashCode(), membership2.GetHashCode());
    }

    #endregion

    #region グループ 6: 統合テスト

    [Fact]
    public void AllProperties_AreCoherent()
    {
        // Arrange
        var deptCode = DepartmentCode.From("DEPT");
        var startDate = new LocalDateTime(new DateTime(2026, 1, 1, 0, 0, 0));
        var expirationDate = new LocalDateTime(new DateTime(2026, 12, 31, 23, 59, 59));

        // Act
        var membership = DepartmentMembership.Create(
            deptCode, isPrimary: true, startDate, expirationDate);

        // Assert
        Assert.NotEqual(Guid.Empty, membership.Id.Value);
        Assert.Equal(deptCode, membership.DepartmentCode);
        Assert.True(membership.IsPrimary);
        Assert.Equal(expirationDate, membership.ExpirationDate);
    }

    #endregion
}

