using System;
using System.Collections.Generic;
using SupportAdvance.Common.Clocks;
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
        var date = new DateTime(year, month, day);
        var createdAt = CreatedAt.From(new LocalDateTime(date));

        // Assert
        Assert.NotNull(createdAt);
        Assert.Equal(new LocalDateTime(date), createdAt.Value);
    }

    [Fact]
    public void From_WithUtcNow_CreatesInstance()
    {
        // Act
        var fixedTime = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified);
        var createdAt = CreatedAt.From(new LocalDateTime(fixedTime));

        // Assert
        Assert.NotNull(createdAt);
        Assert.Equal(new LocalDateTime(fixedTime), createdAt.Value);
    }

    [Fact]
    public void From_WithMinValue_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CreatedAt.From(LocalDateTime.MinValue));
    }

    [Fact]
    public void From_WithMaxValue_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CreatedAt.From(LocalDateTime.MaxValue));
    }

    [Fact]
    public void From_WithMidnight_CreatesInstance()
    {
        // Act
        var date = new DateTime(2025, 1, 1, 0, 0, 0);
        var createdAt = CreatedAt.From(new LocalDateTime(date));

        // Assert
        Assert.NotNull(createdAt);
        Assert.Equal(new LocalDateTime(date), createdAt.Value);
    }

    [Fact]
    public void From_WithLastSecond_CreatesInstance()
    {
        // Act
        var date = new DateTime(2025, 1, 1, 23, 59, 59);
        var createdAt = CreatedAt.From(new LocalDateTime(date));

        // Assert
        Assert.NotNull(createdAt);
        Assert.Equal(new LocalDateTime(date), createdAt.Value);
    }

    #endregion

    #region FromDbValueメソッドテスト

    [Fact]
    public void FromDbValue_WithValidDateTime_CreatesInstance()
    {
        // Arrange
        var dbValue = new DateTime(2025, 1, 1, 10, 30, 0);

        // Act
        var createdAt = CreatedAt.FromDbValue(dbValue);

        // Assert
        Assert.NotNull(createdAt);
        Assert.Equal(new LocalDateTime(dbValue), createdAt.Value);
    }

    [Theory]
    [InlineData(2025, 1, 1)]
    [InlineData(2000, 1, 1)]
    public void FromDbValue_WithVariousDates_CreatesInstance(int year, int month, int day)
    {
        // Arrange
        var dbValue = new DateTime(year, month, day);

        // Act
        var createdAt = CreatedAt.FromDbValue(dbValue);

        // Assert
        Assert.NotNull(createdAt);
        Assert.Equal(new LocalDateTime(dbValue), createdAt.Value);
    }

    #endregion

    #region ToDbValueメソッドテスト

    [Fact]
    public void ToDbValue_ShouldReturnDateTime()
    {
        // Arrange
        var localDateTime = new LocalDateTime(new DateTime(2025, 1, 1, 10, 30, 0));
        var createdAt = CreatedAt.From(localDateTime);

        // Act
        var result = createdAt.ToDbValue();

        // Assert
        Assert.IsType<DateTime>(result);
        Assert.Equal(new DateTime(2025, 1, 1, 10, 30, 0), result);
    }

    [Fact]
    public void ToDbValue_Roundtrip_PreservesValue()
    {
        // Arrange
        var original = new DateTime(2025, 1, 1, 10, 30, 45);
        var createdAt = CreatedAt.FromDbValue(original);

        // Act
        var result = createdAt.ToDbValue();

        // Assert
        Assert.Equal(original, result);
    }

    #endregion

    #region TryFromDbValueメソッドテスト

    [Fact]
    public void TryFromDbValue_WithValidDateTime_ReturnsTrue()
    {
        // Arrange
        var dbValue = new DateTime(2025, 1, 1, 10, 30, 0);

        // Act
        var success = CreatedAt.TryFromDbValue(dbValue, out var createdAt);

        // Assert
        Assert.True(success);
        Assert.NotNull(createdAt);
        Assert.Equal(new LocalDateTime(dbValue), createdAt.Value);
    }

    [Fact]
    public void TryFromDbValue_WithNullValue_ReturnsFalse()
    {
        // Arrange
        DateTime? dbValue = null;

        // Act
        var success = CreatedAt.TryFromDbValue(dbValue, out var createdAt);

        // Assert
        Assert.False(success);
        Assert.Null(createdAt);
    }

    [Fact]
    public void TryFromDbValue_WithValidDateTime_OutputNotNull()
    {
        // Arrange
        var dbValue = new DateTime(2025, 1, 1);

        // Act
        CreatedAt.TryFromDbValue(dbValue, out var result);

        // Assert
        Assert.NotNull(result);
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
        var date = new DateTime(year, month, day);
        bool success = CreatedAt.TryFrom(new LocalDateTime(date), out var createdAt);

        // Assert
        Assert.True(success);
        Assert.NotNull(createdAt);
        Assert.Equal(new LocalDateTime(date), createdAt.Value);
    }

    [Fact]
    public void TryFrom_WithUtcNow_ReturnsTrue()
    {
        // Act
        var fixedTime = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified);
        bool success = CreatedAt.TryFrom(new LocalDateTime(fixedTime), out var createdAt);

        // Assert
        Assert.True(success);
        Assert.NotNull(createdAt);
        Assert.Equal(new LocalDateTime(fixedTime), createdAt.Value);
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
        bool success = CreatedAt.TryFrom(LocalDateTime.MinValue, out var createdAt);

        // Assert
        Assert.False(success);
        Assert.Null(createdAt);
    }

    [Fact]
    public void TryFrom_WithMaxValue_ReturnsFalse()
    {
        // Act
        bool success = CreatedAt.TryFrom(LocalDateTime.MaxValue, out var createdAt);

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
        var date = new DateTime(year, month, day);
        bool success = CreatedAt.TryFrom(new LocalDateTime(date), out var createdAt);

        // Assert
        Assert.True(success);
        Assert.NotNull(createdAt);
        Assert.Equal(new LocalDateTime(date), createdAt.Value);
    }

    [Fact]
    public void TryFrom_NonNullable_WithMinValue_ReturnsFalse()
    {
        // Act
        bool success = CreatedAt.TryFrom(LocalDateTime.MinValue, out var createdAt);

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
        var createdAt1 = CreatedAt.From(new LocalDateTime(date));
        var createdAt2 = CreatedAt.From(new LocalDateTime(date));

        // Act & Assert
        Assert.Equal(createdAt1, createdAt2);
    }

    [Fact]
    public void Equals_Object_WithDifferentDateTime_ReturnsFalse()
    {
        // Arrange
        var createdAt1 = CreatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
        var createdAt2 = CreatedAt.From(new LocalDateTime(new DateTime(2025, 1, 2)));

        // Act & Assert
        Assert.NotEqual(createdAt1, createdAt2);
    }

    [Fact]
    public void Equals_Object_WithNull_ReturnsFalse()
    {
        // Arrange
        var createdAt = CreatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));

        // Act & Assert
        Assert.False(createdAt.Equals(null));
    }

    [Fact]
    public void Equals_Object_WithDifferentType_ReturnsFalse()
    {
        // Arrange
        var createdAt = CreatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
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
        var createdAt1 = CreatedAt.From(new LocalDateTime(date));
        var createdAt2 = CreatedAt.From(new LocalDateTime(date));

        // Act & Assert
        Assert.True(createdAt1.Equals(createdAt2));
    }

    [Fact]
    public void Equals_WithDifferentDateTime_ReturnsFalse()
    {
        // Arrange
        var createdAt1 = CreatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
        var createdAt2 = CreatedAt.From(new LocalDateTime(new DateTime(2025, 1, 2)));

        // Act & Assert
        Assert.False(createdAt1.Equals(createdAt2));
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var createdAt = CreatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));

        // Act & Assert
        Assert.False(createdAt.Equals((CreatedAt?)null));
    }

    [Fact]
    public void Equals_SameInstance_ReturnsTrue()
    {
        // Arrange
        var createdAt1 = CreatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
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
        var createdAt1 = CreatedAt.From(new LocalDateTime(date));
        var createdAt2 = CreatedAt.From(new LocalDateTime(date));

        // Act & Assert
        Assert.Equal(createdAt1.GetHashCode(), createdAt2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_CanBeUsedInDictionary()
    {
        // Arrange
        var createdAt1 = CreatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
        var createdAt2 = CreatedAt.From(new LocalDateTime(new DateTime(2025, 1, 2)));
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
        var createdAt1 = CreatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
        var createdAt2 = CreatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
        var createdAt3 = CreatedAt.From(new LocalDateTime(new DateTime(2025, 1, 2)));

        // Act
        var set = new HashSet<CreatedAt> { createdAt1, createdAt2, createdAt3 };

        // Assert
        Assert.Equal(2, set.Count);
    }

    [Fact]
    public void GetHashCode_WithDifferentDateTime_ReturnsDifferentHashCodes()
    {
        // Arrange
        var createdAt1 = CreatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
        var createdAt2 = CreatedAt.From(new LocalDateTime(new DateTime(2025, 1, 2)));

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
        var createdAt = CreatedAt.From(new LocalDateTime(date));

        // Act
        string result = createdAt.ToString();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("2025", result);
        Assert.Contains("01", result);
    }

    [Fact]
    public void ToString_WithUtcNow_ReturnsValidString()
    {
        // Arrange
        var fixedTime = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Unspecified);
        var createdAt = CreatedAt.From(new LocalDateTime(fixedTime));

        // Act
        string result = createdAt.ToString();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("2025", result);
    }

    #endregion

    #region Valueプロパティテスト

    [Fact]
    public void Value_WhenCreated_ReturnsLocalDateTime()
    {
        // Arrange
        var date = new DateTime(2025, 1, 1, 10, 30, 45);
        var createdAt = CreatedAt.From(new LocalDateTime(date));

        // Act
        var value = createdAt.Value;

        // Assert
        Assert.Equal(new LocalDateTime(date), value);
    }

    [Fact]
    public void Value_MultipleAccess_ReturnsSameValue()
    {
        // Arrange
        var date = new DateTime(2025, 1, 1, 10, 30, 45);
        var createdAt = CreatedAt.From(new LocalDateTime(date));

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
        var createdAt = CreatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
        var originalValue = createdAt.Value;

        // Act
        // Assert
        Assert.Equal(originalValue, createdAt.Value);
    }

    [Fact]
    public void MultipleCalls_CreateDistinctInstances()
    {
        // Arrange & Act
        var createdAt1 = CreatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));
        var createdAt2 = CreatedAt.From(new LocalDateTime(new DateTime(2025, 1, 1)));

        // Assert
        Assert.NotSame(createdAt1, createdAt2);
        Assert.Equal(createdAt1, createdAt2);
    }

    #endregion

    #region 境界値テスト

    [Fact]
    public void From_JustAfterMinValue_CreatesInstance()
    {
        // Act
        var date = new DateTime(0001, 1, 1, 0, 0, 1);
        var createdAt = CreatedAt.From(new LocalDateTime(date));

        // Assert
        Assert.NotNull(createdAt);
        Assert.Equal(new LocalDateTime(date), createdAt.Value);
    }

    [Fact]
    public void From_JustBeforeMaxValue_CreatesInstance()
    {
        // Act
        var date = new DateTime(9999, 12, 31, 23, 59, 58);
        var createdAt = CreatedAt.From(new LocalDateTime(date));

        // Assert
        Assert.NotNull(createdAt);
        Assert.Equal(new LocalDateTime(date), createdAt.Value);
    }

    #endregion

    #region DateTimeKindテスト

    [Fact]
    public void From_WithUnspecifiedKind_CreatesInstance()
    {
        // Arrange
        var unspecifiedDateTime = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

        // Act
        var createdAt = CreatedAt.From(new LocalDateTime(unspecifiedDateTime));

        // Assert
        Assert.NotNull(createdAt);
        Assert.Equal(new LocalDateTime(unspecifiedDateTime), createdAt.Value);
    }

    #endregion
}
