namespace SupportAdvance.SharedKernel.Tests.ValueObjects.Identifiers;

using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Xunit;

/// <summary>
/// DepartmentRowId（部署行ID） 単体テスト
///
/// 必須 ValueObject パターン：0以下は失敗
/// DB スキーマ: m_departments.row_id [bigint] PRIMARY KEY
///
/// テスト観点：
/// - VO-01: From(1以上) は IsSet=true のインスタンスを生成
/// - VO-02: From(0以下) は ArgumentOutOfRangeException を投げる
/// - VO-03: TryFrom で 0以下は false を返す
/// </summary>
public class DepartmentRowIdTests
{
    /// <summary>From(1以上) は IsSet=true のインスタンスを生成</summary>
    [Theory]
    [InlineData(1L)]
    [InlineData(9999L)]
    public void From_WithValidValue_ReturnsInstanceWithIsSetTrue(long validValue)
    {
        // Act
        var deptRowId = DepartmentRowId.From(validValue);

        // Assert
        Assert.NotNull(deptRowId);
        Assert.True(deptRowId.IsSet);
        Assert.Equal(validValue, deptRowId.Value);
    }

    /// <summary>From(0) は ArgumentOutOfRangeException を投げる</summary>
    [Fact]
    public void From_WithZero_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => DepartmentRowId.From(0L));
    }

    /// <summary>From(負の数) は ArgumentOutOfRangeException を投げる</summary>
    [Fact]
    public void From_WithNegativeValue_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => DepartmentRowId.From(-1L));
    }

    /// <summary>TryFrom(1以上) は true と インスタンスを返す</summary>
    [Fact]
    public void TryFrom_WithValidValue_ReturnsTrueAndInstance()
    {
        // Act
        var result = DepartmentRowId.TryFrom(1L, out var deptRowId);

        // Assert
        Assert.True(result);
        Assert.NotNull(deptRowId);
        Assert.True(deptRowId.IsSet);
        Assert.Equal(1L, deptRowId.Value);
    }

    /// <summary>TryFrom(0) は false を返す</summary>
    [Fact]
    public void TryFrom_WithZero_ReturnsFalse()
    {
        // Act
        var result = DepartmentRowId.TryFrom(0L, out var deptRowId);

        // Assert
        Assert.False(result);
    }

    /// <summary>TryFrom(負の数) は false を返す</summary>
    [Fact]
    public void TryFrom_WithNegativeValue_ReturnsFalse()
    {
        // Act
        var result = DepartmentRowId.TryFrom(-1L, out var deptRowId);

        // Assert
        Assert.False(result);
    }

    /// <summary>TryFromDbValue(1以上) は true を返す</summary>
    [Fact]
    public void TryFromDbValue_WithValidValue_ReturnsTrue()
    {
        // Act
        var result = DepartmentRowId.TryFromDbValue(1L, out var deptRowId);

        // Assert
        Assert.True(result);
        Assert.Equal(1L, deptRowId.Value);
    }

    /// <summary>TryFromDbValue(0) は false を返す</summary>
    [Fact]
    public void TryFromDbValue_WithZero_ReturnsFalse()
    {
        // Act
        var result = DepartmentRowId.TryFromDbValue(0L, out var deptRowId);

        // Assert
        Assert.False(result);
    }

    /// <summary>同じ行ID同士は等価</summary>
    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        // Arrange
        var rowId1 = DepartmentRowId.From(1L);
        var rowId2 = DepartmentRowId.From(1L);

        // Act & Assert
        Assert.Equal(rowId1, rowId2);
        Assert.True(rowId1 == rowId2);
    }

    /// <summary>異なる行IDは非等価</summary>
    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        // Arrange
        var rowId1 = DepartmentRowId.From(1L);
        var rowId2 = DepartmentRowId.From(2L);

        // Act & Assert
        Assert.NotEqual(rowId1, rowId2);
        Assert.False(rowId1 == rowId2);
    }

    /// <summary>null との比較は false</summary>
    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var rowId = DepartmentRowId.From(1L);

        // Act & Assert
        Assert.False(rowId.Equals(null));
    }

    /// <summary>ハッシュコード：同じ値は同じハッシュ</summary>
    [Fact]
    public void GetHashCode_SameValues_SameHash()
    {
        // Arrange
        var rowId1 = DepartmentRowId.From(1L);
        var rowId2 = DepartmentRowId.From(1L);

        // Act & Assert
        Assert.Equal(rowId1.GetHashCode(), rowId2.GetHashCode());
    }

    /// <summary>ハッシュコード：異なる値は異なるハッシュ</summary>
    [Fact]
    public void GetHashCode_DifferentValues_DifferentHash()
    {
        // Arrange
        var rowId1 = DepartmentRowId.From(1L);
        var rowId2 = DepartmentRowId.From(2L);

        // Act & Assert
        Assert.NotEqual(rowId1.GetHashCode(), rowId2.GetHashCode());
    }

    /// <summary>ToString は long の文字列表現を返す</summary>
    [Fact]
    public void ToString_ReturnsLongString()
    {
        // Arrange
        var rowId = DepartmentRowId.From(1L);

        // Act
        var result = rowId.ToString();

        // Assert
        Assert.Equal("1", result);
    }
}
