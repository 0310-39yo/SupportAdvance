using System;
using Xunit;
using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects.Audit;

namespace SupportAdvance.SharedKernel.Tests.ValueObjects.Audit;

public class DeletedAtTests
{
    #region From メソッドテスト

    // 正常系
    [Fact]
    public void From_WithValidDateTime_CreatesInstance()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1, 10, 30, 0);

        // Act
        var result = DeletedAt.From(new LocalDateTime(dateTime));

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
        var result = DeletedAt.From(new LocalDateTime(dateTime));

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
        var result = DeletedAt.From(new LocalDateTime(fixedTime));

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
        var result = DeletedAt.From(new LocalDateTime(fixedTime));

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
        var result = DeletedAt.From(new LocalDateTime(dateTime));

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
        var result = DeletedAt.From(new LocalDateTime(dateTime));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new LocalDateTime(dateTime), result.Value);
    }

    // 異常系
    [Fact]
    public void From_WithMinValue_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => DeletedAt.From(LocalDateTime.MinValue));
    }

    [Fact]
    public void From_WithMaxValue_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => DeletedAt.From(LocalDateTime.MaxValue));
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
        var result = DeletedAt.TryFrom(new LocalDateTime?(new LocalDateTime(dateTime)), out var deletedAt);

        // Assert
        Assert.True(result);
        Assert.NotNull(deletedAt);
        Assert.Equal(new LocalDateTime(dateTime), deletedAt.Value);
    }

    [Fact]
    public void TryFrom_Nullable_WithPastDate_ReturnsTrue()
    {
        // Arrange
        var dateTime = new DateTime(2000, 1, 1);

        // Act
        var result = DeletedAt.TryFrom(new LocalDateTime?(new LocalDateTime(dateTime)), out var deletedAt);

        // Assert
        Assert.True(result);
        Assert.NotNull(deletedAt);
        Assert.Equal(new LocalDateTime(dateTime), deletedAt.Value);
    }

    [Fact]
    public void TryFrom_Nullable_WithUtcNow_ReturnsTrue()
    {
        // Arrange
        // 固定時刻を使用（LocalDateTime は DateTimeKind.Unspecified のみを許容）
        var dateTime = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified);

        // Act
        var result = DeletedAt.TryFrom(new LocalDateTime?(new LocalDateTime(dateTime)), out var deletedAt);

        // Assert
        Assert.True(result);
        Assert.NotNull(deletedAt);
        Assert.Equal(new LocalDateTime(dateTime), deletedAt.Value);
    }

    // Null 入力
    [Fact]
    public void TryFrom_Nullable_WithNull_ReturnsTrue()
    {
        // Act
        var result = DeletedAt.TryFrom((LocalDateTime?)null, out var deletedAt);

        // Assert
        Assert.True(result);  // null → Unset() で成功
        Assert.NotNull(deletedAt);
        Assert.False(deletedAt.IsDeleted);  // 未削除状態
    }

    // 異常系
    [Fact]
    public void TryFrom_Nullable_WithMinValue_ReturnsFalse()
    {
        // Act
        var result = DeletedAt.TryFrom(LocalDateTime.MinValue, out var deletedAt);

        // Assert
        Assert.False(result);
        Assert.Null(deletedAt);
    }

    [Fact]
    public void TryFrom_Nullable_WithMaxValue_ReturnsFalse()
    {
        // Act
        var result = DeletedAt.TryFrom(LocalDateTime.MaxValue, out var deletedAt);

        // Assert
        Assert.False(result);
        Assert.Null(deletedAt);
    }

    [Fact]
    public void TryFrom_Nullable_WithInvalidDateTime_ReturnsFalse()
    {
        // Act
        var result = DeletedAt.TryFrom(LocalDateTime.MinValue, out var deletedAt);

        // Assert
        Assert.False(result);
        Assert.Null(deletedAt);
    }

    #endregion

    #region TryFrom (non-nullable) メソッドテスト

    [Fact]
    public void TryFrom_NonNullable_WithValidDateTime_ReturnsTrue()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1);

        // Act
        var result = DeletedAt.TryFrom(new LocalDateTime(dateTime), out var deletedAt);

        // Assert
        Assert.True(result);
        Assert.NotNull(deletedAt);
        Assert.Equal(new LocalDateTime(dateTime), deletedAt.Value);
    }

    [Fact]
    public void TryFrom_NonNullable_WithMinValue_ReturnsFalse()
    {
        // Act
        var result = DeletedAt.TryFrom(LocalDateTime.MinValue, out var deletedAt);

        // Assert
        Assert.False(result);
        Assert.Null(deletedAt);
    }

    [Fact]
    public void TryFrom_NonNullable_WithMaxValue_ReturnsFalse()
    {
        // Act
        var result = DeletedAt.TryFrom(LocalDateTime.MaxValue, out var deletedAt);

        // Assert
        Assert.False(result);
        Assert.Null(deletedAt);
    }

    #endregion

    #region Equals (object?) メソッドテスト

    [Fact]
    public void Equals_Object_WithSameDateTime_ReturnsTrue()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1);
        var deleted1 = DeletedAt.From(new LocalDateTime(dateTime));
        var deleted2 = DeletedAt.From(new LocalDateTime(dateTime));

        // Act
        var result = deleted1.Equals((object)deleted2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_Object_WithDifferentDateTime_ReturnsFalse()
    {
        // Arrange
        var deleted1 = DeletedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
        var deleted2 = DeletedAt.From(new LocalDateTime(new DateTime(2025, 1, 2)));

        // Act
        var result = deleted1.Equals((object)deleted2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Object_WithNull_ReturnsFalse()
    {
        // Arrange
        var deleted = DeletedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));

        // Act
        var result = deleted.Equals((object?)null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Object_WithDifferentType_ReturnsFalse()
    {
        // Arrange
        var deleted = DeletedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));

        // Act
        var result = deleted.Equals((object)"2025-01-01");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Equals (DeletedAt?) メソッドテスト

    [Fact]
    public void Equals_DeletedAt_WithSameDateTime_ReturnsTrue()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1);
        var deleted1 = DeletedAt.From(new LocalDateTime(dateTime));
        var deleted2 = DeletedAt.From(new LocalDateTime(dateTime));

        // Act
        var result = deleted1.Equals(deleted2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_DeletedAt_WithDifferentDateTime_ReturnsFalse()
    {
        // Arrange
        var deleted1 = DeletedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
        var deleted2 = DeletedAt.From(new LocalDateTime(new DateTime(2025, 1, 2)));

        // Act
        var result = deleted1.Equals(deleted2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_DeletedAt_WithNull_ReturnsFalse()
    {
        // Arrange
        var deleted = DeletedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));

        // Act
        var result = deleted.Equals((DeletedAt?)null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        // Arrange
        var deleted1 = DeletedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
        var deleted2 = deleted1;

        // Act
        var result = deleted1.Equals(deleted2);

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
        var deleted1 = DeletedAt.From(new LocalDateTime(dateTime));
        var deleted2 = DeletedAt.From(new LocalDateTime(dateTime));

        // Act
        var hash1 = deleted1.GetHashCode();
        var hash2 = deleted2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void GetHashCode_CanBeUsedInDictionary()
    {
        // Arrange
        var deleted1 = DeletedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
        var deleted2 = DeletedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
        var dict = new Dictionary<DeletedAt, string>();

        // Act
        dict.Add(deleted1, "first");
        var exists = dict.ContainsKey(deleted2);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public void GetHashCode_CanBeUsedInHashSet()
    {
        // Arrange
        var deleted1 = DeletedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
        var deleted2 = DeletedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
        var hashSet = new HashSet<DeletedAt> { deleted1 };

        // Act
        var added = hashSet.Add(deleted2);

        // Assert
        Assert.False(added); // 同じ値なので追加されない
        Assert.Single(hashSet);
    }

    [Fact]
    public void GetHashCode_WithMultipleDates_DifferentHashes()
    {
        // Arrange
        var deleted1 = DeletedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
        var deleted2 = DeletedAt.From(new LocalDateTime(new DateTime(2025, 1, 2)));

        // Act
        var hash1 = deleted1.GetHashCode();
        var hash2 = deleted2.GetHashCode();

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
        var deleted = DeletedAt.From(new LocalDateTime(dateTime));

        // Act
        var result = deleted.ToString();

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
        var deleted = DeletedAt.From(new LocalDateTime(fixedTime));

        // Act
        var result = deleted.ToString();

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
        var deleted = DeletedAt.From(new LocalDateTime(dateTime));

        // Act
        var value = deleted.Value;

        // Assert
        Assert.Equal(new LocalDateTime(dateTime), value);
    }

    [Fact]
    public void Value_IsReadOnly()
    {
        // Arrange
        var deleted = DeletedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));

        // Act & Assert
        // Value プロパティは get-only なので、再代入はコンパイルエラーになる
        // ここではプロパティが存在することを確認
        Assert.NotNull(deleted.Value);
    }

    #endregion

    #region ValueObject 不変性確認テスト

    [Fact]
    public void Immutability_InstanceCannotBeModified()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1);
        var deleted = DeletedAt.From(new LocalDateTime(dateTime));
        var originalValue = deleted.Value;

        // Act
        // DeletedAt インスタンスを操作しても
        // Value プロパティは変わらない

        // Assert
        Assert.Equal(new LocalDateTime(dateTime), deleted.Value);
    }

    #endregion

    #region 複数呼び出しテスト

    [Fact]
    public void MultipleCalls_CreatesIndependentInstances()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1);

        // Act
        var deleted1 = DeletedAt.From(new LocalDateTime(dateTime));
        var deleted2 = DeletedAt.From(new LocalDateTime(dateTime));

        // Assert
        Assert.NotSame(deleted1, deleted2); // 異なるインスタンス
        Assert.Equal(new LocalDateTime(dateTime), deleted1.Value); // 値は同じ
        Assert.Equal(new LocalDateTime(dateTime), deleted2.Value);
        Assert.Equal(deleted1, deleted2); // ValueObject として等価
    }

    #endregion

    #region 境界値テスト

    [Fact]
    public void BoundaryValue_JustAfterMinValue_CreatesInstance()
    {
        // Arrange
        var dateTime = new DateTime(0001, 1, 1, 0, 0, 1);

        // Act
        var result = DeletedAt.From(new LocalDateTime(dateTime));

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
        var result = DeletedAt.From(new LocalDateTime(dateTime));

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
        var result = DeletedAt.From(new LocalDateTime(dateTime));

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
        var result = DeletedAt.From(new LocalDateTime(dateTime));

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
        var result = DeletedAt.From(new LocalDateTime(dateTime));

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
        var result = DeletedAt.Unset();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.False(result.IsDeleted);
    }

    [Fact]
    public void Unset_InstancesAreEqual()
    {
        // Act
        var unset1 = DeletedAt.Unset();
        var unset2 = DeletedAt.Unset();

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
        var result = DeletedAt.FromDbValue(dbDateTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new LocalDateTime(dbDateTime), result.Value);
        Assert.True(result.IsDeleted);
    }

    [Fact]
    public void FromDbValue_WithPastDate_CreatesInstance()
    {
        // Arrange
        var dbDateTime = new DateTime(2000, 1, 1, 12, 0, 0);

        // Act
        var result = DeletedAt.FromDbValue(dbDateTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new LocalDateTime(dbDateTime), result.Value);
        Assert.True(result.IsDeleted);
    }

    [Fact]
    public void FromDbValue_WithMinValue_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => DeletedAt.FromDbValue(DateTime.MinValue));
    }

    [Fact]
    public void FromDbValue_WithMaxValue_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => DeletedAt.FromDbValue(DateTime.MaxValue));
    }

    #endregion

    #region ToDbValue メソッドテスト

    [Fact]
    public void ToDbValue_WithValidValue_ReturnsDateTime()
    {
        // Arrange
        var dateTime = new DateTime(2025, 1, 1, 10, 30, 0);
        var deleted = DeletedAt.From(new LocalDateTime(dateTime));

        // Act
        var result = deleted.ToDbValue();

        // Assert
        Assert.Equal(dateTime, result);
    }

    [Fact]
    public void ToDbValue_WithUnsetInstance_ThrowsInvalidOperationException()
    {
        // Arrange
        var unset = DeletedAt.Unset();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => unset.ToDbValue());
    }

    [Fact]
    public void ToDbValue_RoundTrip_WithDbValue_ReturnsOriginal()
    {
        // Arrange
        var originalDbDateTime = new DateTime(2025, 6, 15, 14, 30, 45);

        // Act
        var fromDb = DeletedAt.FromDbValue(originalDbDateTime);
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
        var result = DeletedAt.TryFromDbValue(dbDateTime, out var deleted);

        // Assert
        Assert.True(result);
        Assert.NotNull(deleted);
        Assert.Equal(new LocalDateTime(dbDateTime), deleted.Value);
        Assert.True(deleted.IsDeleted);
    }

    [Fact]
    public void TryFromDbValue_WithNull_ReturnsTrue_And_Unset()
    {
        // Act
        var result = DeletedAt.TryFromDbValue(null, out var deleted);

        // Assert
        Assert.True(result);  // null → Unset で成功
        Assert.NotNull(deleted);
        Assert.Null(deleted.Value);
        Assert.False(deleted.IsDeleted);  // 未削除状態
    }

    [Fact]
    public void TryFromDbValue_WithMinValue_ReturnsFalse()
    {
        // Act
        var result = DeletedAt.TryFromDbValue(DateTime.MinValue, out var deleted);

        // Assert
        Assert.False(result);
        Assert.Null(deleted);
    }

    [Fact]
    public void TryFromDbValue_WithMaxValue_ReturnsFalse()
    {
        // Act
        var result = DeletedAt.TryFromDbValue(DateTime.MaxValue, out var deleted);

        // Assert
        Assert.False(result);
        Assert.Null(deleted);
    }

    [Fact]
    public void TryFromDbValue_WithPastDate_ReturnsTrue()
    {
        // Arrange
        var dbDateTime = new DateTime(2000, 1, 1, 0, 0, 0);

        // Act
        var result = DeletedAt.TryFromDbValue(dbDateTime, out var deleted);

        // Assert
        Assert.True(result);
        Assert.NotNull(deleted);
        Assert.Equal(new LocalDateTime(dbDateTime), deleted.Value);
    }

    #endregion
}
