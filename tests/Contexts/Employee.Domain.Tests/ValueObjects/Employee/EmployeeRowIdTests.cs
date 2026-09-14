using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Tests.SharedKernel.ValueObjects.Identifiers;

using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using Xunit;

/// <summary>
/// EmployeeRowId ValueObject の単体テスト
/// </summary>
public class EmployeeRowIdTests
{
    #region グループ 1: 生成メソッド（From）

    [Fact]
    public void TestERGEN01_FromMin1ReturnsValidRowId()
    {
        // Act
        var result = EmployeeRowId.From(1L);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1L, result.Value);
    }

    [Fact]
    public void TestERGEN02_FromLargePositiveReturnsValidRowId()
    {
        // Act
        var result = EmployeeRowId.From(9223372036854775807L);  // long.MaxValue

        // Assert
        Assert.NotNull(result);
        Assert.Equal(9223372036854775807L, result.Value);
    }

    [Fact]
    public void TestERGEN03_FromTypicalValueReturnsValidRowId()
    {
        // Act
        var result = EmployeeRowId.From(12345L);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(12345L, result.Value);
    }

    [Fact]
    public void TestERGEN04_FromZeroThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            EmployeeRowId.From(0L));
    }

    [Fact]
    public void TestERGEN05_FromNegativeOneThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            EmployeeRowId.From(-1L));
    }

    [Fact]
    public void TestERGEN06_FromLargeNegativeThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            EmployeeRowId.From(long.MinValue));
    }

    [Theory]
    [InlineData(1L)]
    [InlineData(100L)]
    [InlineData(1000L)]
    [InlineData(10000L)]
    [InlineData(100000L)]
    [InlineData(1000000L)]
    public void TestERGENDataDriven_ValidValuesReturnValidRowIds(long value)
    {
        // Act
        var result = EmployeeRowId.From(value);

        // Assert
        Assert.Equal(value, result.Value);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(-100L)]
    [InlineData(-1000L)]
    public void TestERGENInvalidDataDriven_InvalidValuesThrowException(long value)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            EmployeeRowId.From(value));
    }

    #endregion

    #region グループ 2: 安全な生成（TryFrom）

    [Fact]
    public void TestERTRY01_TryFromMin1ReturnsTrue()
    {
        // Act
        bool success = EmployeeRowId.TryFrom(1L, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(1L, result.Value);
    }

    [Fact]
    public void TestERTRY02_TryFromMaxValueReturnsTrue()
    {
        // Act
        bool success = EmployeeRowId.TryFrom(long.MaxValue, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(long.MaxValue, result.Value);
    }

    [Fact]
    public void TestERTRY03_TryFromZeroReturnsFalse()
    {
        // Act
        bool success = EmployeeRowId.TryFrom(0L, out var result);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestERTRY04_TryFromNegativeOneReturnsFalse()
    {
        // Act
        bool success = EmployeeRowId.TryFrom(-1L, out var result);

        // Assert
        Assert.False(success);
    }

    [Theory]
    [InlineData(1L)]
    [InlineData(100L)]
    [InlineData(1000L)]
    [InlineData(10000L)]
    public void TestERTRYValidDataDriven_ValidValuesReturnTrue(long value)
    {
        // Act
        bool success = EmployeeRowId.TryFrom(value, out var result);

        // Assert
        Assert.True(success);
        Assert.Equal(value, result.Value);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(-100L)]
    public void TestERTRYInvalidDataDriven_InvalidValuesReturnFalse(long value)
    {
        // Act
        bool success = EmployeeRowId.TryFrom(value, out var result);

        // Assert
        Assert.False(success);
    }

    #endregion

    #region グループ 3: DB値変換（TryFromDbValue）

    [Fact]
    public void TestERDBVAL01_TryFromDbValueReturnsTrue()
    {
        // Act
        bool success = EmployeeRowId.TryFromDbValue(12345L, out var result);

        // Assert
        Assert.True(success);
        Assert.Equal(12345L, result.Value);
    }

    [Fact]
    public void TestERDBVAL02_TryFromDbValueMin1ReturnsTrue()
    {
        // Act
        bool success = EmployeeRowId.TryFromDbValue(1L, out var result);

        // Assert
        Assert.True(success);
        Assert.Equal(1L, result.Value);
    }

    [Fact]
    public void TestERDBVAL03_TryFromDbValueMaxReturnsTrue()
    {
        // Act
        bool success = EmployeeRowId.TryFromDbValue(long.MaxValue, out var result);

        // Assert
        Assert.True(success);
        Assert.Equal(long.MaxValue, result.Value);
    }

    [Fact]
    public void TestERDBVAL04_TryFromDbValueZeroReturnsFalse()
    {
        // Act
        bool success = EmployeeRowId.TryFromDbValue(0L, out var result);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TestERDBVAL05_TryFromDbValueNegativeReturnsFalse()
    {
        // Act
        bool success = EmployeeRowId.TryFromDbValue(-1L, out var result);

        // Assert
        Assert.False(success);
    }

    #endregion

    #region グループ 4: 等価性（Equality）

    [Fact]
    public void TestEREQ01_SameValuesAreEqual()
    {
        // Arrange
        var rowId1 = EmployeeRowId.From(12345L);
        var rowId2 = EmployeeRowId.From(12345L);

        // Assert
        Assert.True(rowId1.Equals(rowId2));
        Assert.Equal(rowId1, rowId2);
    }

    [Fact]
    public void TestEREQ02_DifferentValuesAreNotEqual()
    {
        // Arrange
        var rowId1 = EmployeeRowId.From(12345L);
        var rowId2 = EmployeeRowId.From(67890L);

        // Assert
        Assert.False(rowId1.Equals(rowId2));
        Assert.NotEqual(rowId1, rowId2);
    }

    [Fact]
    public void TestEREQ03_ObjectEqualsReturnsTrue()
    {
        // Arrange
        var rowId1 = EmployeeRowId.From(12345L);
        object rowId2 = EmployeeRowId.From(12345L);

        // Assert
        Assert.True(rowId1.Equals(rowId2));
    }

    [Fact]
    public void TestEREQ04_HashCodesAreEqual()
    {
        // Arrange
        var rowId1 = EmployeeRowId.From(12345L);
        var rowId2 = EmployeeRowId.From(12345L);

        // Assert
        Assert.Equal(rowId1.GetHashCode(), rowId2.GetHashCode());
    }

    [Fact]
    public void TestEREQ05_CanBeUsedAsDictionaryKey()
    {
        // Arrange
        var dict = new Dictionary<EmployeeRowId, string>();
        var rowId1 = EmployeeRowId.From(12345L);
        var rowId2 = EmployeeRowId.From(12345L);

        // Act
        dict.Add(rowId1, "Employee123");
        dict[rowId2] = "Employee124";  // Same key

        // Assert
        Assert.Single(dict);
        Assert.Equal("Employee124", dict[rowId1]);
    }

    [Fact]
    public void TestEREQ06_EqualsNullReturnsFalse()
    {
        // Arrange
        var rowId = EmployeeRowId.From(12345L);

        // Assert
        Assert.False(rowId.Equals(null));
    }

    [Fact]
    public void TestEREQ07_EqualsDifferentTypeReturnsFalse()
    {
        // Arrange
        var rowId = EmployeeRowId.From(12345L);

        // Assert
        Assert.False(rowId.Equals(12345L));
    }

    #endregion

    #region グループ 5: 表示形式（Display）

    [Fact]
    public void TestERDISP01_ToStringReturnsNumericString()
    {
        // Arrange
        var rowId = EmployeeRowId.From(12345L);

        // Act
        string result = rowId.ToString();

        // Assert
        Assert.Equal("12345", result);
    }

    [Fact]
    public void TestERDISP02_ToStringMin1()
    {
        // Arrange
        var rowId = EmployeeRowId.From(1L);

        // Act
        string result = rowId.ToString();

        // Assert
        Assert.Equal("1", result);
    }

    [Fact]
    public void TestERDISP03_ToStringLargeValue()
    {
        // Arrange
        var rowId = EmployeeRowId.From(9223372036854775807L);

        // Act
        string result = rowId.ToString();

        // Assert
        Assert.Equal("9223372036854775807", result);
    }

    [Theory]
    [InlineData(1L, "1")]
    [InlineData(100L, "100")]
    [InlineData(12345L, "12345")]
    public void TestERDISPDataDriven_ToStringReturnsExpectedFormat(long value, string expected)
    {
        // Arrange
        var rowId = EmployeeRowId.From(value);

        // Act
        string result = rowId.ToString();

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region グループ 6: 不変性（Immutability）

    [Fact]
    public void TestERIMM01_ValueIsReadOnly()
    {
        // Arrange
        var rowId = EmployeeRowId.From(12345L);

        // Act & Assert
        // Value プロパティは読み取り専用（setter がない）
        Assert.Equal(12345L, rowId.Value);

        // セッターがないため、以下は compile エラー（テスト不可）
        // rowId.Value = 67890L;  // CS0200: Property or indexer cannot be assigned to
    }

    [Fact]
    public void TestERIMM02_ValueNeverChanges()
    {
        // Arrange
        var rowId = EmployeeRowId.From(12345L);

        // Act
        var firstRead = rowId.Value;
        var secondRead = rowId.Value;

        // Assert
        Assert.Equal(firstRead, secondRead);
        Assert.Equal(12345L, firstRead);
    }

    #endregion
}


