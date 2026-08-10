namespace SupportAdvance.Contexts.Employee.Domain.Tests.ValueObjects.Audit;

using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Audit;
using Xunit;

/// <summary>
/// DeletedBy（削除者ID） 単体テスト
///
/// オプション ValueObject パターン：null は Unset に変換して成功を返す
/// 論理削除用：未削除状態を Unset で表現
///
/// テスト観点：
/// - VO-01: From(正の数) は IsSet=true のインスタンスを生成
/// - VO-02: From(0) は ArgumentException を投げる
/// - VO-04: Unset() は IsSet=false のインスタンスを返す
/// - VO-05: TryFrom(null) は true と Unset を返す（null吸収）
/// - VO-06: TryFrom(正の数) は true と インスタンスを返す
/// - VO-15: IsDeleted で削除済み/未削除を判定
/// </summary>
public class DeletedByTests
{
    /// <summary>From(正の数) は IsSet=true のインスタンスを生成</summary>
    [Theory]
    [InlineData(1L)]
    [InlineData(1003L)]
    public void From_WithValidValue_ReturnsInstanceWithIsSetTrue(long validValue)
    {
        // Act
        var deletedBy = DeletedBy.From(validValue);

        // Assert
        Assert.NotNull(deletedBy);
        Assert.True(deletedBy.IsSet);
        Assert.Equal(validValue, deletedBy.Value);
    }

    /// <summary>From(0) は ArgumentException を投げる</summary>
    [Fact]
    public void From_WithZero_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => DeletedBy.From(0L));
    }

    /// <summary>From(負の数) は ArgumentException を投げる</summary>
    [Fact]
    public void From_WithNegativeValue_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => DeletedBy.From(-1L));
    }

    /// <summary>Unset() は IsSet=false のインスタンスを返す（未削除状態）</summary>
    [Fact]
    public void Unset_ReturnsInstanceWithIsSetFalse()
    {
        // Act
        var unset = DeletedBy.Unset();

        // Assert
        Assert.NotNull(unset);
        Assert.False(unset.IsSet);
        Assert.Null(unset.Value);
    }

    /// <summary>TryFrom(null) は true と Unset を返す（null吸収、未削除）</summary>
    [Fact]
    public void TryFrom_WithNull_ReturnsTrueAndUnset()
    {
        // Act
        var result = DeletedBy.TryFrom(null, out var deletedBy);

        // Assert
        Assert.True(result);  // ← 重要：true を返す（失敗ではない）
        Assert.NotNull(deletedBy);
        Assert.False(deletedBy.IsSet);
        Assert.Null(deletedBy.Value);
        Assert.Equal(DeletedBy.Unset(), deletedBy);
    }

    /// <summary>TryFrom(正の数) は true と インスタンスを返す</summary>
    [Fact]
    public void TryFrom_WithValidValue_ReturnsTrueAndInstance()
    {
        // Act
        var result = DeletedBy.TryFrom(1003L, out var deletedBy);

        // Assert
        Assert.True(result);
        Assert.NotNull(deletedBy);
        Assert.True(deletedBy.IsSet);
        Assert.Equal(1003L, deletedBy.Value);
    }

    /// <summary>TryFrom(無効値) は false と Unset を返す</summary>
    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    public void TryFrom_WithInvalidValue_ReturnsFalseAndUnset(long invalidValue)
    {
        // Act
        var result = DeletedBy.TryFrom(invalidValue, out var deletedBy);

        // Assert
        Assert.False(result);
        Assert.NotNull(deletedBy);
        Assert.False(deletedBy.IsSet);
    }

    /// <summary>Unset 同士は等価</summary>
    [Fact]
    public void Equals_WithBothUnset_ReturnsTrue()
    {
        // Arrange
        var unset1 = DeletedBy.Unset();
        var unset2 = DeletedBy.Unset();

        // Act & Assert
        Assert.Equal(unset1, unset2);
        Assert.True(unset1 == unset2);
    }

    /// <summary>IsSet が異なる場合は非等価</summary>
    [Fact]
    public void Equals_WithDifferentIsSet_ReturnsFalse()
    {
        // Arrange
        var deleted = DeletedBy.From(1003L);
        var unset = DeletedBy.Unset();

        // Act & Assert
        Assert.NotEqual(deleted, unset);
        Assert.False(deleted == unset);
    }

    /// <summary>同じ値で IsSet が同じ場合は等価</summary>
    [Fact]
    public void Equals_WithSameValueAndIsSet_ReturnsTrue()
    {
        // Arrange
        var deleted1 = DeletedBy.From(1003L);
        var deleted2 = DeletedBy.From(1003L);

        // Act & Assert
        Assert.Equal(deleted1, deleted2);
        Assert.True(deleted1 == deleted2);
    }

    /// <summary>IsDeleted で削除済み/未削除を判定</summary>
    [Fact]
    public void IsDeleted_ReturnsCorrectState()
    {
        // Arrange
        var deleted = DeletedBy.From(1003L);
        var unset = DeletedBy.Unset();

        // Act & Assert
        Assert.True(deleted.IsDeleted);
        Assert.False(unset.IsDeleted);
    }

    /// <summary>TryFromDbValue で DB NULL が Unset に変換される</summary>
    [Fact]
    public void TryFromDbValue_WithNull_ReturnsTrueAndUnset()
    {
        // Act
        var result = DeletedBy.TryFromDbValue(null, out var deletedBy);

        // Assert
        Assert.True(result);
        Assert.False(deletedBy.IsSet);
    }

    /// <summary>ToString は IsSet=false で "Unset" を返す</summary>
    [Fact]
    public void ToString_WithUnset_ReturnsUnset()
    {
        // Arrange
        var unset = DeletedBy.Unset();

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
        var deleted = DeletedBy.From(1003L);

        // Act
        var result = deleted.ToString();

        // Assert
        Assert.Equal("1003", result);
    }
}


