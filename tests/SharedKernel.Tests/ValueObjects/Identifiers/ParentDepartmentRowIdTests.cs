namespace SupportAdvance.SharedKernel.Tests.ValueObjects.Identifiers;

using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Xunit;

/// <summary>
/// ParentDepartmentRowId（親部署行ID） 単体テスト
///
/// オプション ValueObject パターン：null は Unset に変換して成功を返す
/// DB スキーマ: m_departments.parent_row_id [bigint] NULL
///
/// テスト観点：
/// - VO-01: From(1以上) は IsSet=true のインスタンスを生成
/// - VO-02: From(0以下) は ArgumentException を投げる
/// - VO-04: Unset() は IsSet=false のインスタンスを返す
/// - VO-05: TryFrom(null) は true と Unset を返す（null吸収）
/// </summary>
public class ParentDepartmentRowIdTests
{
    /// <summary>From(1以上) は IsSet=true のインスタンスを生成</summary>
    [Theory]
    [InlineData(1L)]
    [InlineData(9999L)]
    public void From_WithValidValue_ReturnsInstanceWithIsSetTrue(long validValue)
    {
        // Act
        var parentRowId = ParentDepartmentRowId.From(validValue);

        // Assert
        Assert.NotNull(parentRowId);
        Assert.True(parentRowId.IsSet);
        Assert.Equal(validValue, parentRowId.Value);
    }

    /// <summary>From(0) は ArgumentException を投げる</summary>
    [Fact]
    public void From_WithZero_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ParentDepartmentRowId.From(0L));
    }

    /// <summary>From(負の数) は ArgumentException を投げる</summary>
    [Fact]
    public void From_WithNegativeValue_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ParentDepartmentRowId.From(-1L));
    }

    /// <summary>Unset() は IsSet=false のインスタンスを返す（ルート部署）</summary>
    [Fact]
    public void Unset_ReturnsInstanceWithIsSetFalse()
    {
        // Act
        var unset = ParentDepartmentRowId.Unset();

        // Assert
        Assert.NotNull(unset);
        Assert.False(unset.IsSet);
        Assert.Null(unset.Value);
    }

    /// <summary>TryFrom(1以上) は true と インスタンスを返す</summary>
    [Fact]
    public void TryFrom_WithValidValue_ReturnsTrueAndInstance()
    {
        // Act
        var result = ParentDepartmentRowId.TryFrom(1L, out var parentRowId);

        // Assert
        Assert.True(result);
        Assert.NotNull(parentRowId);
        Assert.True(parentRowId.IsSet);
        Assert.Equal(1L, parentRowId.Value);
    }

    /// <summary>TryFrom(null) は true と Unset を返す（null吸収）</summary>
    [Fact]
    public void TryFrom_WithNull_ReturnsTrueAndUnset()
    {
        // Act
        var result = ParentDepartmentRowId.TryFrom(null, out var parentRowId);

        // Assert
        Assert.True(result);  // ← 重要：true を返す（失敗ではない）
        Assert.NotNull(parentRowId);
        Assert.False(parentRowId.IsSet);
        Assert.Null(parentRowId.Value);
        Assert.Equal(ParentDepartmentRowId.Unset(), parentRowId);
    }

    /// <summary>TryFrom(0) は false を返す</summary>
    [Fact]
    public void TryFrom_WithZero_ReturnsFalse()
    {
        // Act
        var result = ParentDepartmentRowId.TryFrom(0L, out var parentRowId);

        // Assert
        Assert.False(result);
    }

    /// <summary>TryFrom(負の数) は false を返す</summary>
    [Fact]
    public void TryFrom_WithNegativeValue_ReturnsFalse()
    {
        // Act
        var result = ParentDepartmentRowId.TryFrom(-1L, out var parentRowId);

        // Assert
        Assert.False(result);
    }

    /// <summary>TryFromDbValue(1以上) は true を返す</summary>
    [Fact]
    public void TryFromDbValue_WithValidValue_ReturnsTrue()
    {
        // Act
        var result = ParentDepartmentRowId.TryFromDbValue(1L, out var parentRowId);

        // Assert
        Assert.True(result);
        Assert.True(parentRowId.IsSet);
        Assert.Equal(1L, parentRowId.Value);
    }

    /// <summary>TryFromDbValue(null) は true と Unset を返す</summary>
    [Fact]
    public void TryFromDbValue_WithNull_ReturnsTrueAndUnset()
    {
        // Act
        var result = ParentDepartmentRowId.TryFromDbValue(null, out var parentRowId);

        // Assert
        Assert.True(result);
        Assert.False(parentRowId.IsSet);
    }

    /// <summary>HasParent が true（親あり）を判定</summary>
    [Fact]
    public void HasParent_WithValue_ReturnsTrue()
    {
        // Arrange
        var parentRowId = ParentDepartmentRowId.From(1L);

        // Act & Assert
        Assert.True(parentRowId.HasParent);
    }

    /// <summary>HasParent が false（親なし）を判定</summary>
    [Fact]
    public void HasParent_WithUnset_ReturnsFalse()
    {
        // Arrange
        var unset = ParentDepartmentRowId.Unset();

        // Act & Assert
        Assert.False(unset.HasParent);
    }

    /// <summary>同じ値同士は等価</summary>
    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        // Arrange
        var parentRowId1 = ParentDepartmentRowId.From(1L);
        var parentRowId2 = ParentDepartmentRowId.From(1L);

        // Act & Assert
        Assert.Equal(parentRowId1, parentRowId2);
        Assert.True(parentRowId1 == parentRowId2);
    }

    /// <summary>異なる値は非等価</summary>
    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        // Arrange
        var parentRowId1 = ParentDepartmentRowId.From(1L);
        var parentRowId2 = ParentDepartmentRowId.From(2L);

        // Act & Assert
        Assert.NotEqual(parentRowId1, parentRowId2);
        Assert.False(parentRowId1 == parentRowId2);
    }

    /// <summary>Unset 同士は等価</summary>
    [Fact]
    public void Equals_BothUnset_ReturnsTrue()
    {
        // Arrange
        var unset1 = ParentDepartmentRowId.Unset();
        var unset2 = ParentDepartmentRowId.Unset();

        // Act & Assert
        Assert.Equal(unset1, unset2);
        Assert.True(unset1 == unset2);
    }

    /// <summary>IsSet が異なる場合は非等価</summary>
    [Fact]
    public void Equals_DifferentIsSet_ReturnsFalse()
    {
        // Arrange
        var parentRowId = ParentDepartmentRowId.From(1L);
        var unset = ParentDepartmentRowId.Unset();

        // Act & Assert
        Assert.NotEqual(parentRowId, unset);
        Assert.False(parentRowId == unset);
    }

    /// <summary>ハッシュコード：同じ値は同じハッシュ</summary>
    [Fact]
    public void GetHashCode_SameValues_SameHash()
    {
        // Arrange
        var parentRowId1 = ParentDepartmentRowId.From(1L);
        var parentRowId2 = ParentDepartmentRowId.From(1L);

        // Act & Assert
        Assert.Equal(parentRowId1.GetHashCode(), parentRowId2.GetHashCode());
    }

    /// <summary>ハッシュコード：異なるIsSetは異なるハッシュ</summary>
    [Fact]
    public void GetHashCode_DifferentIsSet_DifferentHash()
    {
        // Arrange
        var parentRowId = ParentDepartmentRowId.From(1L);
        var unset = ParentDepartmentRowId.Unset();

        // Act & Assert
        Assert.NotEqual(parentRowId.GetHashCode(), unset.GetHashCode());
    }

    /// <summary>ToString は IsSet=false で "Unset" を返す</summary>
    [Fact]
    public void ToString_WithUnset_ReturnsUnset()
    {
        // Arrange
        var unset = ParentDepartmentRowId.Unset();

        // Act
        var result = unset.ToString();

        // Assert
        Assert.Equal("Unset", result);
    }

    /// <summary>ToString は IsSet=true で 値の文字列表現を返す</summary>
    [Fact]
    public void ToString_WithValue_ReturnsStringRepresentation()
    {
        // Arrange
        var parentRowId = ParentDepartmentRowId.From(1L);

        // Act
        var result = parentRowId.ToString();

        // Assert
        Assert.Equal("1", result);
    }
}
