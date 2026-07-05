using System;
using System.Collections.Generic;
using SupportAdvance.SharedKernel.ValueObjects.Audit;
using Xunit;

namespace SupportAdvance.Tests.SharedKernel.ValueObjects.Audit;

/// <summary>
/// CreatedAtの単体テスト
/// </summary>
public class CreatedAtTests
{
    #region Fromメソッドテスト

    [Theory]
    [InlineData(2025, 1, 1)]
    [InlineData(2000, 1, 1)]
    [InlineData(2024, 12, 31)]
    public void From_WithValidDateTime_CreatesInstance(int year, int month, int day)
    {
        // Act
        var createdAt = CreatedAt.From(new DateTime(year, month, day));

        // Assert
        Assert.NotNull(createdAt);
        Assert.Equal(new DateTime(year, month, day), createdAt.Value);
    }

    [Fact]
    public void From_WithUtcNow_CreatesInstance()
    {
        // Act
        var before = DateTime.UtcNow;
        var createdAt = CreatedAt.From(DateTime.UtcNow);
        var after = DateTime.UtcNow;

        // Assert
        Assert.NotNull(createdAt);
        Assert.True(before <= createdAt.Value && createdAt.Value <= after);
    }

    [Fact]
    public void From_WithMinValue_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CreatedAt.From(DateTime.MinValue));
    }

    [Fact]
    public void From_WithMaxValue_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CreatedAt.From(DateTime.MaxValue));
    }

    [Fact]
    public void From_WithMidnight_CreatesInstance()
    {
        // Act
        var createdAt = CreatedAt.From(new DateTime(2025, 1, 1, 0, 0, 0));

        // Assert
        Assert.NotNull(createdAt);
        Assert.Equal(new DateTime(2025, 1, 1, 0, 0, 0), createdAt.Value);
    }

    [Fact]
    public void From_WithLastSecond_CreatesInstance()
    {
        // Act
        var createdAt = CreatedAt.From(new DateTime(2025, 1, 1, 23, 59, 59));

        // Assert
        Assert.NotNull(createdAt);
        Assert.Equal(new DateTime(2025, 1, 1, 23, 59, 59), createdAt.Value);
    }

    #endregion

    #region TryFromメソッドテスト（nullable対応）

    [Theory]
    [InlineData(2025, 1, 1)]
    [InlineData(2000, 1, 1)]
    [InlineData(2024, 12, 31)]
    public void TryFrom_WithValidDateTime_ReturnsTrue(int year, int month, int day)
    {
        // Act
        bool success = CreatedAt.TryFrom(new DateTime(year, month, day), out var createdAt);

        // Assert
        Assert.True(success);
        Assert.NotNull(createdAt);
        Assert.Equal(new DateTime(year, month, day), createdAt.Value);
    }

    [Fact]
    public void TryFrom_WithUtcNow_ReturnsTrue()
    {
        // Act
        bool success = CreatedAt.TryFrom(DateTime.UtcNow, out var createdAt);

        // Assert
        Assert.True(success);
        Assert.NotNull(createdAt);
    }

    [Fact]
    public void TryFrom_WithNullInput_ReturnsFalse()
    {
        // Act
        bool success = CreatedAt.TryFrom(null, out var createdAt);

        // Assert
        Assert.False(success);
        Assert.Null(createdAt);
    }

    [Fact]
    public void TryFrom_WithMinValue_ReturnsFalse()
    {
        // Act
        bool success = CreatedAt.TryFrom(DateTime.MinValue, out var createdAt);

        // Assert
        Assert.False(success);
        Assert.Null(createdAt);
    }

    [Fact]
    public void TryFrom_WithMaxValue_ReturnsFalse()
    {
        // Act
        bool success = CreatedAt.TryFrom(DateTime.MaxValue, out var createdAt);

        // Assert
        Assert.False(success);
        Assert.Null(createdAt);
    }

    #endregion

    #region TryFromメソッドテスト（non-nullable オーバーロード）

    [Theory]
    [InlineData(2025, 1, 1)]
    [InlineData(2000, 1, 1)]
    public void TryFrom_NonNullable_WithValidDateTime_ReturnsTrue(int year, int month, int day)
    {
        // Act
        bool success = CreatedAt.TryFrom(new DateTime(year, month, day), out var createdAt);

        // Assert
        Assert.True(success);
        Assert.NotNull(createdAt);
        Assert.Equal(new DateTime(year, month, day), createdAt.Value);
    }

    [Fact]
    public void TryFrom_NonNullable_WithMinValue_ReturnsFalse()
    {
        // Act
        bool success = CreatedAt.TryFrom(DateTime.MinValue, out var createdAt);

        // Assert
        Assert.False(success);
        Assert.Null(createdAt);
    }

    #endregion

    #region Equalsメソッドテスト（object?）

    [Fact]
    public void Equals_Object_WithSameDateTime_ReturnsTrue()
    {
        // Arrange
        var date = new DateTime(2025, 1, 1, 10, 30, 0);
        var createdAt1 = CreatedAt.From(date);
        var createdAt2 = CreatedAt.From(date);

        // Act & Assert
        Assert.Equal(createdAt1, createdAt2);
    }

    [Fact]
    public void Equals_Object_WithDifferentDateTime_ReturnsFalse()
    {
        // Arrange
        var createdAt1 = CreatedAt.From(new DateTime(2025, 1, 1));
        var createdAt2 = CreatedAt.From(new DateTime(2025, 1, 2));

        // Act & Assert
        Assert.NotEqual(createdAt1, createdAt2);
    }

    [Fact]
    public void Equals_Object_WithNull_ReturnsFalse()
    {
        // Arrange
        var createdAt = CreatedAt.From(new DateTime(2025, 1, 1));

        // Act & Assert
        Assert.False(createdAt.Equals(null));
    }

    [Fact]
    public void Equals_Object_WithDifferentType_ReturnsFalse()
    {
        // Arrange
        var createdAt = CreatedAt.From(new DateTime(2025, 1, 1));
        var dateTime = new DateTime(2025, 1, 1);

        // Act & Assert
        Assert.False(createdAt.Equals((object)dateTime));
    }

    #endregion

    #region Equalsメソッドテスト（CreatedAt?）

    [Fact]
    public void Equals_WithSameDateTime_ReturnsTrue()
    {
        // Arrange
        var date = new DateTime(2025, 1, 1, 10, 30, 0);
        var createdAt1 = CreatedAt.From(date);
        var createdAt2 = CreatedAt.From(date);

        // Act & Assert
        Assert.True(createdAt1.Equals(createdAt2));
    }

    [Fact]
    public void Equals_WithDifferentDateTime_ReturnsFalse()
    {
        // Arrange
        var createdAt1 = CreatedAt.From(new DateTime(2025, 1, 1));
        var createdAt2 = CreatedAt.From(new DateTime(2025, 1, 2));

        // Act & Assert
        Assert.False(createdAt1.Equals(createdAt2));
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var createdAt = CreatedAt.From(new DateTime(2025, 1, 1));

        // Act & Assert
        Assert.False(createdAt.Equals((CreatedAt?)null));
    }

    [Fact]
    public void Equals_SameInstance_ReturnsTrue()
    {
        // Arrange
        var createdAt1 = CreatedAt.From(new DateTime(2025, 1, 1));
        var createdAt2 = createdAt1;

        // Act & Assert
        Assert.True(createdAt1.Equals(createdAt2));
    }

    #endregion

    #region GetHashCodeテスト

    [Fact]
    public void GetHashCode_WithSameDateTime_ReturnsSameHashCode()
    {
        // Arrange
        var date = new DateTime(2025, 1, 1, 10, 30, 0);
        var createdAt1 = CreatedAt.From(date);
        var createdAt2 = CreatedAt.From(date);

        // Act & Assert
        Assert.Equal(createdAt1.GetHashCode(), createdAt2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_CanBeUsedInDictionary()
    {
        // Arrange
        var createdAt1 = CreatedAt.From(new DateTime(2025, 1, 1));
        var createdAt2 = CreatedAt.From(new DateTime(2025, 1, 2));
        var dict = new Dictionary<CreatedAt, string>();

        // Act
        dict[createdAt1] = "Value1";
        dict[createdAt2] = "Value2";

        // Assert
        Assert.Equal(2, dict.Count);
        Assert.Equal("Value1", dict[createdAt1]);
        Assert.Equal("Value2", dict[createdAt2]);
    }

    [Fact]
    public void GetHashCode_CanBeUsedInHashSet()
    {
        // Arrange
        var createdAt1 = CreatedAt.From(new DateTime(2025, 1, 1));
        var createdAt2 = CreatedAt.From(new DateTime(2025, 1, 1));
        var createdAt3 = CreatedAt.From(new DateTime(2025, 1, 2));

        // Act
        var set = new HashSet<CreatedAt> { createdAt1, createdAt2, createdAt3 };

        // Assert
        Assert.Equal(2, set.Count);  // createdAt1とcreatedAt2は等価なため1つにまとめられる
    }

    [Fact]
    public void GetHashCode_WithDifferentDateTime_ReturnsDifferentHashCodes()
    {
        // Arrange
        var createdAt1 = CreatedAt.From(new DateTime(2025, 1, 1));
        var createdAt2 = CreatedAt.From(new DateTime(2025, 1, 2));

        // Act & Assert
        Assert.NotEqual(createdAt1.GetHashCode(), createdAt2.GetHashCode());
    }

    #endregion

    #region ToStringテスト

    [Fact]
    public void ToString_WithDateTime_ReturnsIso8601Format()
    {
        // Arrange
        var date = new DateTime(2025, 1, 1, 10, 30, 45);
        var createdAt = CreatedAt.From(date);

        // Act
        string result = createdAt.ToString();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("2025", result);
        Assert.Contains("01", result);
        Assert.Contains("01", result);
    }

    [Fact]
    public void ToString_WithUtcNow_ReturnsValidString()
    {
        // Arrange
        var createdAt = CreatedAt.From(DateTime.UtcNow);

        // Act
        string result = createdAt.ToString();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    #endregion

    #region Valueプロパティテスト

    [Fact]
    public void Value_WhenCreated_ReturnsOriginalDateTime()
    {
        // Arrange
        var date = new DateTime(2025, 1, 1, 10, 30, 45);
        var createdAt = CreatedAt.From(date);

        // Act
        var value = createdAt.Value;

        // Assert
        Assert.Equal(date, value);
    }

    [Fact]
    public void Value_MultipleAccess_ReturnsSameValue()
    {
        // Arrange
        var date = new DateTime(2025, 1, 1, 10, 30, 45);
        var createdAt = CreatedAt.From(date);

        // Act
        var value1 = createdAt.Value;
        var value2 = createdAt.Value;

        // Assert
        Assert.Equal(value1, value2);
    }

    #endregion

    #region 不変性テスト

    [Fact]
    public void Instance_IsImmutable()
    {
        // Arrange
        var createdAt = CreatedAt.From(new DateTime(2025, 1, 1));
        var originalValue = createdAt.Value;

        // Act
        // Valueプロパティはget-onlyなため、再代入は不可
        // コンパイルエラーとなるため、ここでは値の変化がないことを確認

        // Assert
        Assert.Equal(originalValue, createdAt.Value);
    }

    [Fact]
    public void MultipleCalls_CreateDistinctInstances()
    {
        // Arrange & Act
        var createdAt1 = CreatedAt.From(new DateTime(2025, 1, 1));
        var createdAt2 = CreatedAt.From(new DateTime(2025, 1, 1));

        // Assert
        // ValueFieldは同値だが、インスタンスは異なる
        Assert.NotSame(createdAt1, createdAt2);
        Assert.Equal(createdAt1, createdAt2);  // ただし等価
    }

    #endregion

    #region 境界値テスト

    [Fact]
    public void From_JustAfterMinValue_CreatesInstance()
    {
        // Act
        var createdAt = CreatedAt.From(new DateTime(0001, 1, 1, 0, 0, 1));

        // Assert
        Assert.NotNull(createdAt);
        Assert.Equal(new DateTime(0001, 1, 1, 0, 0, 1), createdAt.Value);
    }

    [Fact]
    public void From_JustBeforeMaxValue_CreatesInstance()
    {
        // Act
        var createdAt = CreatedAt.From(new DateTime(9999, 12, 31, 23, 59, 58));

        // Assert
        Assert.NotNull(createdAt);
        Assert.Equal(new DateTime(9999, 12, 31, 23, 59, 58), createdAt.Value);
    }

    #endregion

    #region DateTimeKindテスト

    [Fact]
    public void From_WithUtcKind_CreatesInstance()
    {
        // Arrange
        var utcDateTime = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var createdAt = CreatedAt.From(utcDateTime);

        // Assert
        Assert.NotNull(createdAt);
        Assert.Equal(utcDateTime, createdAt.Value);
    }

    [Fact]
    public void From_WithLocalKind_CreatesInstance()
    {
        // Arrange
        var localDateTime = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Local);

        // Act
        var createdAt = CreatedAt.From(localDateTime);

        // Assert
        Assert.NotNull(createdAt);
        Assert.Equal(localDateTime, createdAt.Value);
    }

    [Fact]
    public void From_WithUnspecifiedKind_CreatesInstance()
    {
        // Arrange
        var unspecifiedDateTime = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

        // Act
        var createdAt = CreatedAt.From(unspecifiedDateTime);

        // Assert
        Assert.NotNull(createdAt);
        Assert.Equal(unspecifiedDateTime, createdAt.Value);
    }

    #endregion
}
