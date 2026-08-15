using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Role;

namespace SupportAdvance.Tests.SharedKernel.ValueObjects.Identifiers;

using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using Xunit;

/// <summary>
/// RoleAssignmentRowId ValueObject の単体テスト
/// </summary>
public class RoleAssignmentRowIdTests
{
    #region グループ 1: 生成メソッド（From）

    [Fact]
    public void TestRAGEN01_FromMin1ReturnsValidRowId()
    {
        // Act
        var result = RoleAssignmentRowId.From(1L);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1L, result.Value);
    }

    [Fact]
    public void TestRAGEN02_FromLargePositiveReturnsValidRowId()
    {
        // Act
        var result = RoleAssignmentRowId.From(9223372036854775807L);  // long.MaxValue

        // Assert
        Assert.NotNull(result);
        Assert.Equal(9223372036854775807L, result.Value);
    }

    [Fact]
    public void TestRAGEN03_FromTypicalValueReturnsValidRowId()
    {
        // Act
        var result = RoleAssignmentRowId.From(12345L);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(12345L, result.Value);
    }

    [Fact]
    public void TestRAGEN04_FromZeroThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RoleAssignmentRowId.From(0L));
    }

    [Fact]
    public void TestRAGEN05_FromNegativeOneThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RoleAssignmentRowId.From(-1L));
    }

    [Theory]
    [InlineData(1L)]
    [InlineData(100L)]
    [InlineData(1000L)]
    [InlineData(10000L)]
    [InlineData(100000L)]
    public void TestRAGENDataDriven_ValidValuesReturnValidRowIds(long value)
    {
        // Act
        var result = RoleAssignmentRowId.From(value);

        // Assert
        Assert.Equal(value, result.Value);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(-100L)]
    public void TestRAGENInvalidDataDriven_InvalidValuesThrowException(long value)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RoleAssignmentRowId.From(value));
    }

    #endregion

    #region グループ 2: 等価性（Equals）

    [Fact]
    public void TestRAEQ01_SameValueAreEqual()
    {
        // Arrange
        var rowId1 = RoleAssignmentRowId.From(100L);
        var rowId2 = RoleAssignmentRowId.From(100L);

        // Assert
        Assert.Equal(rowId1, rowId2);
    }

    [Fact]
    public void TestRAEQ02_DifferentValuesAreNotEqual()
    {
        // Arrange
        var rowId1 = RoleAssignmentRowId.From(100L);
        var rowId2 = RoleAssignmentRowId.From(200L);

        // Assert
        Assert.NotEqual(rowId1, rowId2);
    }

    [Fact]
    public void TestRAEQ03_SameInstanceEqualsItself()
    {
        // Arrange
        var rowId = RoleAssignmentRowId.From(100L);

        // Assert
        Assert.Equal(rowId, rowId);
    }

    #endregion

    #region グループ 3: ハッシュコード（GetHashCode）

    [Fact]
    public void TestRAHASH01_EqualValuesHaveSameHashCode()
    {
        // Arrange
        var rowId1 = RoleAssignmentRowId.From(100L);
        var rowId2 = RoleAssignmentRowId.From(100L);

        // Assert
        Assert.Equal(rowId1.GetHashCode(), rowId2.GetHashCode());
    }

    [Fact]
    public void TestRAHASH02_CanBeUsedInHashSet()
    {
        // Arrange
        var rowId1 = RoleAssignmentRowId.From(100L);
        var rowId2 = RoleAssignmentRowId.From(100L);
        var rowId3 = RoleAssignmentRowId.From(200L);

        // Act
        var hashSet = new HashSet<RoleAssignmentRowId> { rowId1, rowId2, rowId3 };

        // Assert
        Assert.Equal(2, hashSet.Count);  // rowId1 と rowId2 は同じ値なので 1 つにマージされる
    }

    #endregion

    #region グループ 4: 文字列表現（ToString）

    [Fact]
    public void TestRASTR01_ToStringReturnsFormattedValue()
    {
        // Arrange
        var rowId = RoleAssignmentRowId.From(12345L);

        // Act
        var result = rowId.ToString();

        // Assert
        Assert.Equal("RoleAssignmentRowId(12345)", result);
    }

    [Theory]
    [InlineData(1L, "RoleAssignmentRowId(1)")]
    [InlineData(100L, "RoleAssignmentRowId(100)")]
    [InlineData(12345L, "RoleAssignmentRowId(12345)")]
    public void TestRASTRDataDriven_ToStringReturnsExpectedFormat(long value, string expected)
    {
        // Arrange
        var rowId = RoleAssignmentRowId.From(value);

        // Act
        var result = rowId.ToString();

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion
}
