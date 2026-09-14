using SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;

namespace SupportAdvance.Tests.SharedKernel.ValueObjects.Identifiers;

using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using Xunit;

/// <summary>
/// DepartmentMembershipRowId ValueObject の単体テスト
/// </summary>
public class DepartmentMembershipRowIdTests
{
    #region グループ 1: 生成メソッド（From）

    [Fact]
    public void TestDMGEN01_FromMin1ReturnsValidRowId()
    {
        // Act
        var result = DepartmentMembershipRowId.From(1L);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1L, result.Value);
    }

    [Fact]
    public void TestDMGEN02_FromLargePositiveReturnsValidRowId()
    {
        // Act
        var result = DepartmentMembershipRowId.From(9223372036854775807L);  // long.MaxValue

        // Assert
        Assert.NotNull(result);
        Assert.Equal(9223372036854775807L, result.Value);
    }

    [Fact]
    public void TestDMGEN03_FromTypicalValueReturnsValidRowId()
    {
        // Act
        var result = DepartmentMembershipRowId.From(12345L);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(12345L, result.Value);
    }

    [Fact]
    public void TestDMGEN04_FromZeroThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DepartmentMembershipRowId.From(0L));
    }

    [Fact]
    public void TestDMGEN05_FromNegativeOneThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DepartmentMembershipRowId.From(-1L));
    }

    [Theory]
    [InlineData(1L)]
    [InlineData(100L)]
    [InlineData(1000L)]
    [InlineData(10000L)]
    [InlineData(100000L)]
    public void TestDMGENDataDriven_ValidValuesReturnValidRowIds(long value)
    {
        // Act
        var result = DepartmentMembershipRowId.From(value);

        // Assert
        Assert.Equal(value, result.Value);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(-100L)]
    public void TestDMGENInvalidDataDriven_InvalidValuesThrowException(long value)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DepartmentMembershipRowId.From(value));
    }

    #endregion

    #region グループ 2: 等価性（Equals）

    [Fact]
    public void TestDMEQ01_SameValueAreEqual()
    {
        // Arrange
        var rowId1 = DepartmentMembershipRowId.From(100L);
        var rowId2 = DepartmentMembershipRowId.From(100L);

        // Assert
        Assert.Equal(rowId1, rowId2);
    }

    [Fact]
    public void TestDMEQ02_DifferentValuesAreNotEqual()
    {
        // Arrange
        var rowId1 = DepartmentMembershipRowId.From(100L);
        var rowId2 = DepartmentMembershipRowId.From(200L);

        // Assert
        Assert.NotEqual(rowId1, rowId2);
    }

    [Fact]
    public void TestDMEQ03_SameInstanceEqualsItself()
    {
        // Arrange
        var rowId = DepartmentMembershipRowId.From(100L);

        // Assert
        Assert.Equal(rowId, rowId);
    }

    #endregion

    #region グループ 3: ハッシュコード（GetHashCode）

    [Fact]
    public void TestDMHASH01_EqualValuesHaveSameHashCode()
    {
        // Arrange
        var rowId1 = DepartmentMembershipRowId.From(100L);
        var rowId2 = DepartmentMembershipRowId.From(100L);

        // Assert
        Assert.Equal(rowId1.GetHashCode(), rowId2.GetHashCode());
    }

    [Fact]
    public void TestDMHASH02_CanBeUsedInHashSet()
    {
        // Arrange
        var rowId1 = DepartmentMembershipRowId.From(100L);
        var rowId2 = DepartmentMembershipRowId.From(100L);
        var rowId3 = DepartmentMembershipRowId.From(200L);

        // Act
        var hashSet = new HashSet<DepartmentMembershipRowId> { rowId1, rowId2, rowId3 };

        // Assert
        Assert.Equal(2, hashSet.Count);  // rowId1 と rowId2 は同じ値なので 1 つにマージされる
    }

    #endregion

    #region グループ 4: 文字列表現（ToString）

    [Fact]
    public void TestDMSTR01_ToStringReturnsFormattedValue()
    {
        // Arrange
        var rowId = DepartmentMembershipRowId.From(12345L);

        // Act
        var result = rowId.ToString();

        // Assert
        Assert.Equal("DepartmentMembershipRowId(12345)", result);
    }

    [Theory]
    [InlineData(1L, "DepartmentMembershipRowId(1)")]
    [InlineData(100L, "DepartmentMembershipRowId(100)")]
    [InlineData(12345L, "DepartmentMembershipRowId(12345)")]
    public void TestDMSTRDataDriven_ToStringReturnsExpectedFormat(long value, string expected)
    {
        // Arrange
        var rowId = DepartmentMembershipRowId.From(value);

        // Act
        var result = rowId.ToString();

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion
}
