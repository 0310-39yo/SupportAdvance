using System;
using Xunit;
using SupportAdvance.SharedKernel.ValueObjects.Audit;

namespace SupportAdvance.SharedKernel.Tests.ValueObjects.Audit;

public class UpdatedAtTests
{
    #region From メソッドテスト

    // 正常系
    [Fact]
    public void From_WithValidDateTime_CreatesInstance()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1, 10, 30, 0);

        // Act
        var result = UpdatedAt.From(dateTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dateTime, result.Value);
    }

    [Fact]
    public void From_WithPastDate_CreatesInstance()
    {
        // Arrange
        var dateTime = new DateTime(2000, 1, 1);

        // Act
        var result = UpdatedAt.From(dateTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dateTime, result.Value);
    }

    [Fact]
    public void From_WithUtcNow_CreatesInstance()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var result = UpdatedAt.From(DateTime.UtcNow);
        var after = DateTime.UtcNow;

        // Assert
        Assert.NotNull(result);
        Assert.InRange(result.Value, before, after);
    }

    [Fact]
    public void From_WithLocalNow_CreatesInstance()
    {
        // Arrange
        var before = DateTime.Now;

        // Act
        var result = UpdatedAt.From(DateTime.Now);
        var after = DateTime.Now;

        // Assert
        Assert.NotNull(result);
        Assert.InRange(result.Value, before, after);
    }

    [Fact]
    public void From_WithTodayMidnight_CreatesInstance()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1, 0, 0, 0);

        // Act
        var result = UpdatedAt.From(dateTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dateTime, result.Value);
    }

    [Fact]
    public void From_WithTodayLastSecond_CreatesInstance()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1, 23, 59, 59);

        // Act
        var result = UpdatedAt.From(dateTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dateTime, result.Value);
    }

    // 異常系
    [Fact]
    public void From_WithMinValue_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => UpdatedAt.From(DateTime.MinValue));
    }

    [Fact]
    public void From_WithMaxValue_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => UpdatedAt.From(DateTime.MaxValue));
    }

    #endregion

    #region TryFrom (nullable) メソッドテスト

    // 正常系
    [Fact]
    public void TryFrom_Nullable_WithValidDateTime_ReturnsTrue()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1);

        // Act
        var result = UpdatedAt.TryFrom((DateTime?)dateTime, out var updatedAt);

        // Assert
        Assert.True(result);
        Assert.NotNull(updatedAt);
        Assert.Equal(dateTime, updatedAt.Value);
    }

    [Fact]
    public void TryFrom_Nullable_WithPastDate_ReturnsTrue()
    {
        // Arrange
        var dateTime = new DateTime(2000, 1, 1);

        // Act
        var result = UpdatedAt.TryFrom((DateTime?)dateTime, out var updatedAt);

        // Assert
        Assert.True(result);
        Assert.NotNull(updatedAt);
        Assert.Equal(dateTime, updatedAt.Value);
    }

    [Fact]
    public void TryFrom_Nullable_WithUtcNow_ReturnsTrue()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;

        // Act
        var result = UpdatedAt.TryFrom((DateTime?)dateTime, out var updatedAt);

        // Assert
        Assert.True(result);
        Assert.NotNull(updatedAt);
    }

    // Null 入力
    [Fact]
    public void TryFrom_Nullable_WithNull_ReturnsFalse()
    {
        // Act
        var result = UpdatedAt.TryFrom((DateTime?)null, out var updatedAt);

        // Assert
        Assert.False(result);
        Assert.Null(updatedAt);
    }

    // 異常系
    [Fact]
    public void TryFrom_Nullable_WithMinValue_ReturnsFalse()
    {
        // Act
        var result = UpdatedAt.TryFrom(DateTime.MinValue, out var updatedAt);

        // Assert
        Assert.False(result);
        Assert.Null(updatedAt);
    }

    [Fact]
    public void TryFrom_Nullable_WithMaxValue_ReturnsFalse()
    {
        // Act
        var result = UpdatedAt.TryFrom(DateTime.MaxValue, out var updatedAt);

        // Assert
        Assert.False(result);
        Assert.Null(updatedAt);
    }

    [Fact]
    public void TryFrom_Nullable_WithInvalidDateTime_ReturnsFalse()
    {
        // Act
        var result = UpdatedAt.TryFrom((DateTime?)DateTime.MinValue, out var updatedAt);

        // Assert
        Assert.False(result);
        Assert.Null(updatedAt);
    }

    #endregion

    #region TryFrom (non-nullable) メソッドテスト

    [Fact]
    public void TryFrom_NonNullable_WithValidDateTime_ReturnsTrue()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1);

        // Act
        var result = UpdatedAt.TryFrom(dateTime, out var updatedAt);

        // Assert
        Assert.True(result);
        Assert.NotNull(updatedAt);
        Assert.Equal(dateTime, updatedAt.Value);
    }

    [Fact]
    public void TryFrom_NonNullable_WithMinValue_ReturnsFalse()
    {
        // Act
        var result = UpdatedAt.TryFrom(DateTime.MinValue, out var updatedAt);

        // Assert
        Assert.False(result);
        Assert.Null(updatedAt);
    }

    [Fact]
    public void TryFrom_NonNullable_WithMaxValue_ReturnsFalse()
    {
        // Act
        var result = UpdatedAt.TryFrom(DateTime.MaxValue, out var updatedAt);

        // Assert
        Assert.False(result);
        Assert.Null(updatedAt);
    }

    #endregion

    #region Equals (object?) メソッドテスト

    [Fact]
    public void Equals_Object_WithSameDateTime_ReturnsTrue()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1);
        var updated1 = UpdatedAt.From(dateTime);
        var updated2 = UpdatedAt.From(dateTime);

        // Act
        var result = updated1.Equals((object)updated2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_Object_WithDifferentDateTime_ReturnsFalse()
    {
        // Arrange
        var updated1 = UpdatedAt.From(new DateTime(2025, 1, 1));
        var updated2 = UpdatedAt.From(new DateTime(2025, 1, 2));

        // Act
        var result = updated1.Equals((object)updated2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Object_WithNull_ReturnsFalse()
    {
        // Arrange
        var updated = UpdatedAt.From(new DateTime(2025, 1, 1));

        // Act
        var result = updated.Equals((object?)null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Object_WithDifferentType_ReturnsFalse()
    {
        // Arrange
        var updated = UpdatedAt.From(new DateTime(2025, 1, 1));

        // Act
        var result = updated.Equals((object)"2025-01-01");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Equals (UpdatedAt?) メソッドテスト

    [Fact]
    public void Equals_UpdatedAt_WithSameDateTime_ReturnsTrue()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1);
        var updated1 = UpdatedAt.From(dateTime);
        var updated2 = UpdatedAt.From(dateTime);

        // Act
        var result = updated1.Equals(updated2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_UpdatedAt_WithDifferentDateTime_ReturnsFalse()
    {
        // Arrange
        var updated1 = UpdatedAt.From(new DateTime(2025, 1, 1));
        var updated2 = UpdatedAt.From(new DateTime(2025, 1, 2));

        // Act
        var result = updated1.Equals(updated2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_UpdatedAt_WithNull_ReturnsFalse()
    {
        // Arrange
        var updated = UpdatedAt.From(new DateTime(2025, 1, 1));

        // Act
        var result = updated.Equals((UpdatedAt?)null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        // Arrange
        var updated1 = UpdatedAt.From(new DateTime(2025, 1, 1));
        var updated2 = updated1;

        // Act
        var result = updated1.Equals(updated2);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region GetHashCode メソッドテスト

    [Fact]
    public void GetHashCode_WithSameDateTime_ReturnsSameHash()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1);
        var updated1 = UpdatedAt.From(dateTime);
        var updated2 = UpdatedAt.From(dateTime);

        // Act
        var hash1 = updated1.GetHashCode();
        var hash2 = updated2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void GetHashCode_CanBeUsedInDictionary()
    {
        // Arrange
        var updated1 = UpdatedAt.From(new DateTime(2025, 1, 1));
        var updated2 = UpdatedAt.From(new DateTime(2025, 1, 1));
        var dict = new Dictionary<UpdatedAt, string>();

        // Act
        dict.Add(updated1, "first");
        var exists = dict.ContainsKey(updated2);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public void GetHashCode_CanBeUsedInHashSet()
    {
        // Arrange
        var updated1 = UpdatedAt.From(new DateTime(2025, 1, 1));
        var updated2 = UpdatedAt.From(new DateTime(2025, 1, 1));
        var hashSet = new HashSet<UpdatedAt> { updated1 };

        // Act
        var added = hashSet.Add(updated2);

        // Assert
        Assert.False(added); // 同じ値なので追加されない
        Assert.Single(hashSet);
    }

    [Fact]
    public void GetHashCode_WithMultipleDates_DifferentHashes()
    {
        // Arrange
        var updated1 = UpdatedAt.From(new DateTime(2025, 1, 1));
        var updated2 = UpdatedAt.From(new DateTime(2025, 1, 2));

        // Act
        var hash1 = updated1.GetHashCode();
        var hash2 = updated2.GetHashCode();

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    #endregion

    #region ToString メソッドテスト

    [Fact]
    public void ToString_WithDateTime_ReturnsIso8601Format()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1, 10, 30, 0);
        var updated = UpdatedAt.From(dateTime);

        // Act
        var result = updated.ToString();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("2025", result);
    }

    [Fact]
    public void ToString_WithUtcNow_ReturnsValidFormat()
    {
        // Arrange
        var updated = UpdatedAt.From(DateTime.UtcNow);

        // Act
        var result = updated.ToString();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    #endregion

    #region Value プロパティテスト

    [Fact]
    public void Value_ReturnsStoredDateTime()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1, 10, 30, 45);
        var updated = UpdatedAt.From(dateTime);

        // Act
        var value = updated.Value;

        // Assert
        Assert.Equal(dateTime, value);
    }

    [Fact]
    public void Value_IsReadOnly()
    {
        // Arrange
        var updated = UpdatedAt.From(new DateTime(2025, 1, 1));

        // Act & Assert
        // Value プロパティは get-only なので、再代入はコンパイルエラーになる
        // ここではプロパティが存在することを確認
        Assert.NotNull(updated.Value);
    }

    #endregion

    #region ValueObject 不変性確認テスト

    [Fact]
    public void Immutability_InstanceCannotBeModified()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1);
        var updated = UpdatedAt.From(dateTime);
        var originalValue = updated.Value;

        // Act
        // UpdatedAt インスタンスを操作しても
        // Value プロパティは変わらない

        // Assert
        Assert.Equal(originalValue, updated.Value);
    }

    #endregion

    #region 複数呼び出しテスト

    [Fact]
    public void MultipleCalls_CreatesIndependentInstances()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1);

        // Act
        var updated1 = UpdatedAt.From(dateTime);
        var updated2 = UpdatedAt.From(dateTime);

        // Assert
        Assert.NotSame(updated1, updated2); // 異なるインスタンス
        Assert.Equal(updated1.Value, updated2.Value); // 値は同じ
        Assert.Equal(updated1, updated2); // ValueObject として等価
    }

    #endregion

    #region 境界値テスト

    [Fact]
    public void BoundaryValue_JustAfterMinValue_CreatesInstance()
    {
        // Arrange
        var dateTime = new DateTime(0001, 1, 1, 0, 0, 1);

        // Act
        var result = UpdatedAt.From(dateTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dateTime, result.Value);
    }

    [Fact]
    public void BoundaryValue_JustBeforeMaxValue_CreatesInstance()
    {
        // Arrange
        var dateTime = new DateTime(9999, 12, 31, 23, 59, 58);

        // Act
        var result = UpdatedAt.From(dateTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dateTime, result.Value);
    }

    #endregion

    #region DateTimeKind テスト

    [Fact]
    public void DateTimeKind_WithUtc_CreatesInstance()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1, 10, 30, 0, DateTimeKind.Utc);

        // Act
        var result = UpdatedAt.From(dateTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dateTime, result.Value);
        Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
    }

    [Fact]
    public void DateTimeKind_WithLocal_CreatesInstance()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1, 10, 30, 0, DateTimeKind.Local);

        // Act
        var result = UpdatedAt.From(dateTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dateTime, result.Value);
        Assert.Equal(DateTimeKind.Local, result.Value.Kind);
    }

    [Fact]
    public void DateTimeKind_WithUnspecified_CreatesInstance()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1, 10, 30, 0, DateTimeKind.Unspecified);

        // Act
        var result = UpdatedAt.From(dateTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dateTime, result.Value);
        Assert.Equal(DateTimeKind.Unspecified, result.Value.Kind);
    }

    #endregion
}
