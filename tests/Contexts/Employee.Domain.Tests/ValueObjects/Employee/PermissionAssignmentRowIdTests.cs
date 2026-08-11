namespace SupportAdvance.Tests.SharedKernel.ValueObjects.Identifiers;

using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using Xunit;

/// <summary>
/// PermissionAssignmentRowId ValueObject の単体テスト
/// </summary>
public class PermissionAssignmentRowIdTests
{
    #region グループ 1: 生成メソッド（From）

    [Fact]
    public void TestPAGEN01_FromMin1ReturnsValidRowId()
    {
        // Act
        var result = PermissionAssignmentRowId.From(1L);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1L, result.Value);
    }

    [Fact]
    public void TestPAGEN02_FromLargePositiveReturnsValidRowId()
    {
        // Act
        var result = PermissionAssignmentRowId.From(9223372036854775807L);  // long.MaxValue

        // Assert
        Assert.NotNull(result);
        Assert.Equal(9223372036854775807L, result.Value);
    }

    [Fact]
    public void TestPAGEN03_FromTypicalValueReturnsValidRowId()
    {
        // Act
        var result = PermissionAssignmentRowId.From(12345L);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(12345L, result.Value);
    }

    [Fact]
    public void TestPAGEN04_FromZeroThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PermissionAssignmentRowId.From(0L));
    }

    [Fact]
    public void TestPAGEN05_FromNegativeOneThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PermissionAssignmentRowId.From(-1L));
    }

    [Theory]
    [InlineData(1L)]
    [InlineData(100L)]
    [InlineData(1000L)]
    [InlineData(10000L)]
    [InlineData(100000L)]
    public void TestPAGENDataDriven_ValidValuesReturnValidRowIds(long value)
    {
        // Act
        var result = PermissionAssignmentRowId.From(value);

        // Assert
        Assert.Equal(value, result.Value);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(-100L)]
    public void TestPAGENInvalidDataDriven_InvalidValuesThrowException(long value)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PermissionAssignmentRowId.From(value));
    }

    #endregion

    #region グループ 2: 等価性（Equals）

    [Fact]
    public void TestPAEQ01_SameValueAreEqual()
    {
        // Arrange
        var rowId1 = PermissionAssignmentRowId.From(100L);
        var rowId2 = PermissionAssignmentRowId.From(100L);

        // Assert
        Assert.Equal(rowId1, rowId2);
    }

    [Fact]
    public void TestPAEQ02_DifferentValuesAreNotEqual()
    {
        // Arrange
        var rowId1 = PermissionAssignmentRowId.From(100L);
        var rowId2 = PermissionAssignmentRowId.From(200L);

        // Assert
        Assert.NotEqual(rowId1, rowId2);
    }

    [Fact]
    public void TestPAEQ03_SameInstanceEqualsItself()
    {
        // Arrange
        var rowId = PermissionAssignmentRowId.From(100L);

        // Assert
        Assert.Equal(rowId, rowId);
    }

    #endregion

    #region グループ 3: ハッシュコード（GetHashCode）

    [Fact]
    public void TestPAHASH01_EqualValuesHaveSameHashCode()
    {
        // Arrange
        var rowId1 = PermissionAssignmentRowId.From(100L);
        var rowId2 = PermissionAssignmentRowId.From(100L);

        // Assert
        Assert.Equal(rowId1.GetHashCode(), rowId2.GetHashCode());
    }

    [Fact]
    public void TestPAHASH02_CanBeUsedInHashSet()
    {
        // Arrange
        var rowId1 = PermissionAssignmentRowId.From(100L);
        var rowId2 = PermissionAssignmentRowId.From(100L);
        var rowId3 = PermissionAssignmentRowId.From(200L);

        // Act
        var hashSet = new HashSet<PermissionAssignmentRowId> { rowId1, rowId2, rowId3 };

        // Assert
        Assert.Equal(2, hashSet.Count);  // rowId1 と rowId2 は同じ値なので 1 つにマージされる
    }

    #endregion

    #region グループ 4: 文字列表現（ToString）

    [Fact]
    public void TestPASTR01_ToStringReturnsFormattedValue()
    {
        // Arrange
        var rowId = PermissionAssignmentRowId.From(12345L);

        // Act
        var result = rowId.ToString();

        // Assert
        Assert.Equal("PermissionAssignmentRowId(12345)", result);
    }

    [Theory]
    [InlineData(1L, "PermissionAssignmentRowId(1)")]
    [InlineData(100L, "PermissionAssignmentRowId(100)")]
    [InlineData(12345L, "PermissionAssignmentRowId(12345)")]
    public void TestPASTRDataDriven_ToStringReturnsExpectedFormat(long value, string expected)
    {
        // Arrange
        var rowId = PermissionAssignmentRowId.From(value);

        // Act
        var result = rowId.ToString();

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion
}
