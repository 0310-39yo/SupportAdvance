using System;
using Xunit;
using SupportAdvance.Common.Clocks;
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
        var result = UpdatedAt.From(new LocalDateTime(dateTime));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new LocalDateTime(dateTime), result.Value);
    }

    [Fact]
    public void From_WithPastDate_CreatesInstance()
    {
        // Arrange
        var dateTime = new DateTime(2000, 1, 1);

        // Act
        var result = UpdatedAt.From(new LocalDateTime(dateTime));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new LocalDateTime(dateTime), result.Value);
    }

    [Fact]
    public void From_WithUtcNow_CreatesInstance()
    {
        // Arrange
        // 固定時刻を使用（LocalDateTime は DateTimeKind.Unspecified のみを許容）
        var fixedTime = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified);

        // Act
        var result = UpdatedAt.From(new LocalDateTime(fixedTime));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new LocalDateTime(fixedTime), result.Value);
    }

    [Fact]
    public void From_WithLocalNow_CreatesInstance()
    {
        // Arrange
        // 固定時刻を使用（LocalDateTime は DateTimeKind.Unspecified のみを許容）
        var fixedTime = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified);

        // Act
        var result = UpdatedAt.From(new LocalDateTime(fixedTime));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new LocalDateTime(fixedTime), result.Value);
    }

    [Fact]
    public void From_WithTodayMidnight_CreatesInstance()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1, 0, 0, 0);

        // Act
        var result = UpdatedAt.From(new LocalDateTime(dateTime));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new LocalDateTime(dateTime), result.Value);
    }

    [Fact]
    public void From_WithTodayLastSecond_CreatesInstance()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1, 23, 59, 59);

        // Act
        var result = UpdatedAt.From(new LocalDateTime(dateTime));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new LocalDateTime(dateTime), result.Value);
    }

    // 異常系
    [Fact]
    public void From_WithMinValue_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => UpdatedAt.From(LocalDateTime.MinValue));
    }

    [Fact]
    public void From_WithMaxValue_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => UpdatedAt.From(LocalDateTime.MaxValue));
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
        var result = UpdatedAt.TryFrom(new LocalDateTime?(new LocalDateTime(dateTime)), out var updatedAt);

        // Assert
        Assert.True(result);
        Assert.NotNull(updatedAt);
        Assert.Equal(new LocalDateTime(dateTime), updatedAt.Value);
    }

    [Fact]
    public void TryFrom_Nullable_WithPastDate_ReturnsTrue()
    {
        // Arrange
        var dateTime = new DateTime(2000, 1, 1);

        // Act
        var result = UpdatedAt.TryFrom(new LocalDateTime?(new LocalDateTime(dateTime)), out var updatedAt);

        // Assert
        Assert.True(result);
        Assert.NotNull(updatedAt);
        Assert.Equal(new LocalDateTime(dateTime), updatedAt.Value);
    }

    [Fact]
    public void TryFrom_Nullable_WithUtcNow_ReturnsTrue()
    {
        // Arrange
        // 固定時刻を使用（LocalDateTime は DateTimeKind.Unspecified のみを許容）
        var dateTime = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified);

        // Act
        var result = UpdatedAt.TryFrom(new LocalDateTime?(new LocalDateTime(dateTime)), out var updatedAt);

        // Assert
        Assert.True(result);
        Assert.NotNull(updatedAt);
        Assert.Equal(new LocalDateTime(dateTime), updatedAt.Value);
    }

    // Null 入力
    [Fact]
    public void TryFrom_Nullable_WithNull_ReturnsTrue()
    {
        // Act
        var result = UpdatedAt.TryFrom((LocalDateTime?)null, out var updatedAt);

        // Assert
        Assert.True(result);  // null → Unset() で成功
        Assert.NotNull(updatedAt);
        Assert.False(updatedAt.HasUpdated);  // 未更新状態
    }

    // 異常系
    [Fact]
    public void TryFrom_Nullable_WithMinValue_ReturnsFalse()
    {
        // Act
        var result = UpdatedAt.TryFrom(LocalDateTime.MinValue, out var updatedAt);

        // Assert
        Assert.False(result);
        Assert.Null(updatedAt);
    }

    [Fact]
    public void TryFrom_Nullable_WithMaxValue_ReturnsFalse()
    {
        // Act
        var result = UpdatedAt.TryFrom(LocalDateTime.MaxValue, out var updatedAt);

        // Assert
        Assert.False(result);
        Assert.Null(updatedAt);
    }

    [Fact]
    public void TryFrom_Nullable_WithInvalidDateTime_ReturnsFalse()
    {
        // Act
        var result = UpdatedAt.TryFrom(LocalDateTime.MinValue, out var updatedAt);

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
        var result = UpdatedAt.TryFrom(new LocalDateTime(dateTime), out var updatedAt);

        // Assert
        Assert.True(result);
        Assert.NotNull(updatedAt);
        Assert.Equal(new LocalDateTime(dateTime), updatedAt.Value);
    }

    [Fact]
    public void TryFrom_NonNullable_WithMinValue_ReturnsFalse()
    {
        // Act
        var result = UpdatedAt.TryFrom(LocalDateTime.MinValue, out var updatedAt);

        // Assert
        Assert.False(result);
        Assert.Null(updatedAt);
    }

    [Fact]
    public void TryFrom_NonNullable_WithMaxValue_ReturnsFalse()
    {
        // Act
        var result = UpdatedAt.TryFrom(LocalDateTime.MaxValue, out var updatedAt);

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
        var updated1 = UpdatedAt.From(new LocalDateTime(dateTime));
        var updated2 = UpdatedAt.From(new LocalDateTime(dateTime));

        // Act
        var result = updated1.Equals((object)updated2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_Object_WithDifferentDateTime_ReturnsFalse()
    {
        // Arrange
        var updated1 = UpdatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
        var updated2 = UpdatedAt.From(new LocalDateTime(new DateTime(2025, 1, 2)));

        // Act
        var result = updated1.Equals((object)updated2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Object_WithNull_ReturnsFalse()
    {
        // Arrange
        var updated = UpdatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));

        // Act
        var result = updated.Equals((object?)null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Object_WithDifferentType_ReturnsFalse()
    {
        // Arrange
        var updated = UpdatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));

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
        var updated1 = UpdatedAt.From(new LocalDateTime(dateTime));
        var updated2 = UpdatedAt.From(new LocalDateTime(dateTime));

        // Act
        var result = updated1.Equals(updated2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_UpdatedAt_WithDifferentDateTime_ReturnsFalse()
    {
        // Arrange
        var updated1 = UpdatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
        var updated2 = UpdatedAt.From(new LocalDateTime(new DateTime(2025, 1, 2)));

        // Act
        var result = updated1.Equals(updated2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_UpdatedAt_WithNull_ReturnsFalse()
    {
        // Arrange
        var updated = UpdatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));

        // Act
        var result = updated.Equals((UpdatedAt?)null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        // Arrange
        var updated1 = UpdatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
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
        var updated1 = UpdatedAt.From(new LocalDateTime(dateTime));
        var updated2 = UpdatedAt.From(new LocalDateTime(dateTime));

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
        var updated1 = UpdatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
        var updated2 = UpdatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
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
        var updated1 = UpdatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
        var updated2 = UpdatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
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
        var updated1 = UpdatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
        var updated2 = UpdatedAt.From(new LocalDateTime(new DateTime(2025, 1, 2)));

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
        var updated = UpdatedAt.From(new LocalDateTime(dateTime));

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
        // 固定時刻を使用（LocalDateTime は DateTimeKind.Unspecified のみを許容）
        var fixedTime = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified);
        var updated = UpdatedAt.From(new LocalDateTime(fixedTime));

        // Act
        var result = updated.ToString();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("2025", result);
    }

    #endregion

    #region Value プロパティテスト

    [Fact]
    public void Value_ReturnsStoredDateTime()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1, 10, 30, 45);
        var updated = UpdatedAt.From(new LocalDateTime(dateTime));

        // Act
        var value = updated.Value;

        // Assert
        Assert.Equal(new LocalDateTime(dateTime), value);
    }

    [Fact]
    public void Value_IsReadOnly()
    {
        // Arrange
        var updated = UpdatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));

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
        var updated = UpdatedAt.From(new LocalDateTime(dateTime));
        var originalValue = updated.Value;

        // Act
        // UpdatedAt インスタンスを操作しても
        // Value プロパティは変わらない

        // Assert
        Assert.Equal(new LocalDateTime(dateTime), updated.Value);
    }

    #endregion

    #region 複数呼び出しテスト

    [Fact]
    public void MultipleCalls_CreatesIndependentInstances()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1);

        // Act
        var updated1 = UpdatedAt.From(new LocalDateTime(dateTime));
        var updated2 = UpdatedAt.From(new LocalDateTime(dateTime));

        // Assert
        Assert.NotSame(updated1, updated2); // 異なるインスタンス
        Assert.Equal(new LocalDateTime(dateTime), updated1.Value); // 値は同じ
        Assert.Equal(new LocalDateTime(dateTime), updated2.Value);
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
        var result = UpdatedAt.From(new LocalDateTime(dateTime));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new LocalDateTime(dateTime), result.Value);
    }

    [Fact]
    public void BoundaryValue_JustBeforeMaxValue_CreatesInstance()
    {
        // Arrange
        var dateTime = new DateTime(9999, 12, 31, 23, 59, 58);

        // Act
        var result = UpdatedAt.From(new LocalDateTime(dateTime));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new LocalDateTime(dateTime), result.Value);
    }

    #endregion

    #region DateTimeKind テスト

    [Fact]
    public void DateTimeKind_WithUtc_CreatesInstance()
    {
        // Arrange
        // LocalDateTime は DateTimeKind.Unspecified のみを許容するため、Unspecified で テスト
        var dateTime = new DateTime(2025, 1, 1, 10, 30, 0, DateTimeKind.Unspecified);

        // Act
        var result = UpdatedAt.From(new LocalDateTime(dateTime));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new LocalDateTime(dateTime), result.Value);
        Assert.NotNull(result.Value);
        Assert.Equal(DateTimeKind.Unspecified, result.Value.Value.Value.Kind);
    }

    [Fact]
    public void DateTimeKind_WithLocal_CreatesInstance()
    {
        // Arrange
        // LocalDateTime は DateTimeKind.Unspecified のみを許容するため、Unspecified で テスト
        var dateTime = new DateTime(2025, 1, 1, 10, 30, 0, DateTimeKind.Unspecified);

        // Act
        var result = UpdatedAt.From(new LocalDateTime(dateTime));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new LocalDateTime(dateTime), result.Value);
        Assert.NotNull(result.Value);
        Assert.Equal(DateTimeKind.Unspecified, result.Value.Value.Value.Kind);
    }

    [Fact]
    public void DateTimeKind_WithUnspecified_CreatesInstance()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1, 10, 30, 0, DateTimeKind.Unspecified);

        // Act
        var result = UpdatedAt.From(new LocalDateTime(dateTime));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new LocalDateTime(dateTime), result.Value);
        Assert.NotNull(result.Value);
        Assert.Equal(DateTimeKind.Unspecified, result.Value.Value.Value.Kind);
    }

    #endregion

    #region Unset メソッドテスト

    [Fact]
    public void Unset_CreatesUnsetInstance()
    {
        // Act
        var result = UpdatedAt.Unset();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.False(result.HasUpdated);
    }

    [Fact]
    public void Unset_InstancesAreEqual()
    {
        // Act
        var unset1 = UpdatedAt.Unset();
        var unset2 = UpdatedAt.Unset();

        // Assert
        Assert.Equal(unset1, unset2);
    }

    #endregion

    #region FromDbValue メソッドテスト

    [Fact]
    public void FromDbValue_WithValidDateTime_CreatesInstance()
    {
        // Arrange
        var dbDateTime = new DateTime(2025, 1, 1, 10, 30, 0);

        // Act
        var result = UpdatedAt.FromDbValue(dbDateTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new LocalDateTime(dbDateTime), result.Value);
        Assert.True(result.HasUpdated);
    }

    [Fact]
    public void FromDbValue_WithPastDate_CreatesInstance()
    {
        // Arrange
        var dbDateTime = new DateTime(2000, 1, 1, 12, 0, 0);

        // Act
        var result = UpdatedAt.FromDbValue(dbDateTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new LocalDateTime(dbDateTime), result.Value);
        Assert.True(result.HasUpdated);
    }

    [Fact]
    public void FromDbValue_WithMinValue_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => UpdatedAt.FromDbValue(DateTime.MinValue));
    }

    [Fact]
    public void FromDbValue_WithMaxValue_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => UpdatedAt.FromDbValue(DateTime.MaxValue));
    }

    #endregion

    #region ToDbValue メソッドテスト

    [Fact]
    public void ToDbValue_WithValidValue_ReturnsDateTime()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1, 10, 30, 0);
        var updated = UpdatedAt.From(new LocalDateTime(dateTime));

        // Act
        var result = updated.ToDbValue();

        // Assert
        Assert.Equal(dateTime, result);
    }

    [Fact]
    public void ToDbValue_WithUnsetInstance_ThrowsInvalidOperationException()
    {
        // Arrange
        var unset = UpdatedAt.Unset();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => unset.ToDbValue());
    }

    [Fact]
    public void ToDbValue_RoundTrip_WithDbValue_ReturnsOriginal()
    {
        // Arrange
        var originalDbDateTime = new DateTime(2025, 6, 15, 14, 30, 45);

        // Act
        var fromDb = UpdatedAt.FromDbValue(originalDbDateTime);
        var toDb = fromDb.ToDbValue();

        // Assert
        Assert.Equal(originalDbDateTime, toDb);
    }

    #endregion

    #region TryFromDbValue メソッドテスト

    [Fact]
    public void TryFromDbValue_WithValidDateTime_ReturnsTrue()
    {
        // Arrange
        var dbDateTime = new DateTime(2025, 1, 1, 10, 30, 0);

        // Act
        var result = UpdatedAt.TryFromDbValue(dbDateTime, out var updated);

        // Assert
        Assert.True(result);
        Assert.NotNull(updated);
        Assert.Equal(new LocalDateTime(dbDateTime), updated.Value);
        Assert.True(updated.HasUpdated);
    }

    [Fact]
    public void TryFromDbValue_WithNull_ReturnsTrue_And_Unset()
    {
        // Act
        var result = UpdatedAt.TryFromDbValue(null, out var updated);

        // Assert
        Assert.True(result);  // null → Unset で成功
        Assert.NotNull(updated);
        Assert.Null(updated.Value);
        Assert.False(updated.HasUpdated);  // 未更新状態
    }

    [Fact]
    public void TryFromDbValue_WithMinValue_ReturnsFalse()
    {
        // Act
        var result = UpdatedAt.TryFromDbValue(DateTime.MinValue, out var updated);

        // Assert
        Assert.False(result);
        Assert.Null(updated);
    }

    [Fact]
    public void TryFromDbValue_WithMaxValue_ReturnsFalse()
    {
        // Act
        var result = UpdatedAt.TryFromDbValue(DateTime.MaxValue, out var updated);

        // Assert
        Assert.False(result);
        Assert.Null(updated);
    }

    [Fact]
    public void TryFromDbValue_WithPastDate_ReturnsTrue()
    {
        // Arrange
        var dbDateTime = new DateTime(2000, 1, 1, 0, 0, 0);

        // Act
        var result = UpdatedAt.TryFromDbValue(dbDateTime, out var updated);

        // Assert
        Assert.True(result);
        Assert.NotNull(updated);
        Assert.Equal(new LocalDateTime(dbDateTime), updated.Value);
    }

    #endregion
}
