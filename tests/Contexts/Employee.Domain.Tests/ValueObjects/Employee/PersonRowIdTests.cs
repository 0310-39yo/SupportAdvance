namespace SupportAdvance.Contexts.Employee.Domain.Tests.ValueObjects.Employee;

using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using Xunit;

/// <summary>
/// PersonRowId ValueObject の単体テスト
/// </summary>
public class PersonRowIdTests
{
    #region グループ 1: 生成メソッド（From）

    [Fact]
    public void TestPRGEN01_FromMin1ReturnsValidRowId()
    {
        var result = PersonRowId.From(1L);
        Assert.NotNull(result);
        Assert.Equal(1L, result.Value);
    }

    [Fact]
    public void TestPRGEN02_FromLargePositiveReturnsValidRowId()
    {
        var result = PersonRowId.From(9223372036854775807L);
        Assert.NotNull(result);
        Assert.Equal(9223372036854775807L, result.Value);
    }

    [Fact]
    public void TestPRGEN03_FromTypicalValueReturnsValidRowId()
    {
        var result = PersonRowId.From(12345L);
        Assert.NotNull(result);
        Assert.Equal(12345L, result.Value);
    }

    [Fact]
    public void TestPRGEN04_FromZeroThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PersonRowId.From(0L));
    }

    [Fact]
    public void TestPRGEN05_FromNegativeOneThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PersonRowId.From(-1L));
    }

    [Fact]
    public void TestPRGEN06_FromLargeNegativeThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PersonRowId.From(long.MinValue));
    }

    [Theory]
    [InlineData(1L)]
    [InlineData(100L)]
    [InlineData(1000L)]
    [InlineData(10000L)]
    public void TestPRGENDataDriven_ValidValuesReturnValidRowIds(long value)
    {
        var result = PersonRowId.From(value);
        Assert.Equal(value, result.Value);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(-100L)]
    public void TestPRGENInvalidDataDriven_InvalidValuesThrowException(long value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PersonRowId.From(value));
    }

    #endregion

    #region グループ 2: 安全な生成（TryFrom）

    [Fact]
    public void TestPRTRY01_TryFromMin1ReturnsTrue()
    {
        bool success = PersonRowId.TryFrom(1L, out var result);
        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(1L, result.Value);
    }

    [Fact]
    public void TestPRTRY02_TryFromMaxValueReturnsTrue()
    {
        bool success = PersonRowId.TryFrom(long.MaxValue, out var result);
        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(long.MaxValue, result.Value);
    }

    [Fact]
    public void TestPRTRY03_TryFromZeroReturnsFalse()
    {
        bool success = PersonRowId.TryFrom(0L, out var result);
        Assert.False(success);
    }

    [Fact]
    public void TestPRTRY04_TryFromNegativeOneReturnsFalse()
    {
        bool success = PersonRowId.TryFrom(-1L, out var result);
        Assert.False(success);
    }

    [Theory]
    [InlineData(1L)]
    [InlineData(100L)]
    [InlineData(1000L)]
    public void TestPRTRYValidDataDriven_ValidValuesReturnTrue(long value)
    {
        bool success = PersonRowId.TryFrom(value, out var result);
        Assert.True(success);
        Assert.Equal(value, result.Value);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    public void TestPRTRYInvalidDataDriven_InvalidValuesReturnFalse(long value)
    {
        bool success = PersonRowId.TryFrom(value, out var result);
        Assert.False(success);
    }

    #endregion

    #region グループ 3: DB値変換（TryFromDbValue）

    [Fact]
    public void TestPRDBVAL01_TryFromDbValueReturnsTrue()
    {
        bool success = PersonRowId.TryFromDbValue(12345L, out var result);
        Assert.True(success);
        Assert.Equal(12345L, result.Value);
    }

    [Fact]
    public void TestPRDBVAL02_TryFromDbValueMin1ReturnsTrue()
    {
        bool success = PersonRowId.TryFromDbValue(1L, out var result);
        Assert.True(success);
        Assert.Equal(1L, result.Value);
    }

    [Fact]
    public void TestPRDBVAL03_TryFromDbValueMaxReturnsTrue()
    {
        bool success = PersonRowId.TryFromDbValue(long.MaxValue, out var result);
        Assert.True(success);
        Assert.Equal(long.MaxValue, result.Value);
    }

    [Fact]
    public void TestPRDBVAL04_TryFromDbValueZeroReturnsFalse()
    {
        bool success = PersonRowId.TryFromDbValue(0L, out var result);
        Assert.False(success);
    }

    [Fact]
    public void TestPRDBVAL05_TryFromDbValueNegativeReturnsFalse()
    {
        bool success = PersonRowId.TryFromDbValue(-1L, out var result);
        Assert.False(success);
    }

    #endregion

    #region グループ 4: 等価性（Equality）

    [Fact]
    public void TestPREQ01_SameValuesAreEqual()
    {
        var rowId1 = PersonRowId.From(12345L);
        var rowId2 = PersonRowId.From(12345L);
        Assert.True(rowId1.Equals(rowId2));
        Assert.Equal(rowId1, rowId2);
    }

    [Fact]
    public void TestPREQ02_DifferentValuesAreNotEqual()
    {
        var rowId1 = PersonRowId.From(12345L);
        var rowId2 = PersonRowId.From(67890L);
        Assert.False(rowId1.Equals(rowId2));
        Assert.NotEqual(rowId1, rowId2);
    }

    [Fact]
    public void TestPREQ03_ObjectEqualsReturnsTrue()
    {
        var rowId1 = PersonRowId.From(12345L);
        object rowId2 = PersonRowId.From(12345L);
        Assert.True(rowId1.Equals(rowId2));
    }

    [Fact]
    public void TestPREQ04_HashCodesAreEqual()
    {
        var rowId1 = PersonRowId.From(12345L);
        var rowId2 = PersonRowId.From(12345L);
        Assert.Equal(rowId1.GetHashCode(), rowId2.GetHashCode());
    }

    [Fact]
    public void TestPREQ05_CanBeUsedAsDictionaryKey()
    {
        var dict = new Dictionary<PersonRowId, string>();
        var rowId1 = PersonRowId.From(12345L);
        var rowId2 = PersonRowId.From(12345L);
        dict.Add(rowId1, "Person123");
        dict[rowId2] = "Person124";
        Assert.Single(dict);
        Assert.Equal("Person124", dict[rowId1]);
    }

    [Fact]
    public void TestPREQ06_EqualsNullReturnsFalse()
    {
        var rowId = PersonRowId.From(12345L);
        Assert.False(rowId.Equals(null));
    }

    [Fact]
    public void TestPREQ07_EqualsDifferentTypeReturnsFalse()
    {
        var rowId = PersonRowId.From(12345L);
        Assert.False(rowId.Equals(12345L));
    }

    #endregion

    #region グループ 5: 表示形式（Display）

    [Fact]
    public void TestPRDISP01_ToStringReturnsNumericString()
    {
        var rowId = PersonRowId.From(12345L);
        string result = rowId.ToString();
        Assert.Equal("12345", result);
    }

    [Fact]
    public void TestPRDISP02_ToStringMin1()
    {
        var rowId = PersonRowId.From(1L);
        string result = rowId.ToString();
        Assert.Equal("1", result);
    }

    [Fact]
    public void TestPRDISP03_ToStringLargeValue()
    {
        var rowId = PersonRowId.From(9223372036854775807L);
        string result = rowId.ToString();
        Assert.Equal("9223372036854775807", result);
    }

    [Theory]
    [InlineData(1L, "1")]
    [InlineData(100L, "100")]
    [InlineData(12345L, "12345")]
    public void TestPRDISPDataDriven_ToStringReturnsExpectedFormat(long value, string expected)
    {
        var rowId = PersonRowId.From(value);
        string result = rowId.ToString();
        Assert.Equal(expected, result);
    }

    #endregion

    #region グループ 6: 不変性（Immutability）

    [Fact]
    public void TestPRIMM01_ValueIsReadOnly()
    {
        var rowId = PersonRowId.From(12345L);
        Assert.Equal(12345L, rowId.Value);
    }

    [Fact]
    public void TestPRIMM02_ValueNeverChanges()
    {
        var rowId = PersonRowId.From(12345L);
        var firstRead = rowId.Value;
        var secondRead = rowId.Value;
        Assert.Equal(firstRead, secondRead);
        Assert.Equal(12345L, firstRead);
    }

    #endregion
}

