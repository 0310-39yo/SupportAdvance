namespace SupportAdvance.SharedKernel.Tests.ValueObjects.Audit;

using SupportAdvance.SharedKernel.ValueObjects.Audit;
using Xunit;

/// <summary>
/// UpdatedBy（更新者ID） 単体テスト
///
/// オプション ValueObject パターン：null は Unset に変換して成功を返す
///
/// テスト観点：
/// - VO-01: From(正の数) は IsSet=true のインスタンスを生成
/// - VO-02: From(0) は ArgumentException を投げる
/// - VO-04: Unset() は IsSet=false のインスタンスを返す
/// - VO-05: TryFrom(null) は true と Unset を返す（null吸収）
/// - VO-06: TryFrom(正の数) は true と インスタンスを返す
/// - VO-07: TryFrom(無効値) は false と Unset を返す
/// - VO-13: Unset 同士は等価
/// - VO-15: HasUpdated で更新済み/未更新を判定
/// </summary>
public class UpdatedByTests
{
    /// <summary>From(正の数) は IsSet=true のインスタンスを生成</summary>
    [Theory]
    [InlineData(1L)]
    [InlineData(1002L)]
    public void From_WithValidValue_ReturnsInstanceWithIsSetTrue(long validValue)
    {
        // Act
        var updatedBy = UpdatedBy.From(validValue);

        // Assert
        Assert.NotNull(updatedBy);
        Assert.True(updatedBy.IsSet);
        Assert.Equal(validValue, updatedBy.Value);
    }

    /// <summary>From(0) は ArgumentException を投げる</summary>
    [Fact]
    public void From_WithZero_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => UpdatedBy.From(0L));
    }

    /// <summary>From(負の数) は ArgumentException を投げる</summary>
    [Fact]
    public void From_WithNegativeValue_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => UpdatedBy.From(-1L));
    }

    /// <summary>Unset() は IsSet=false のインスタンスを返す</summary>
    [Fact]
    public void Unset_ReturnsInstanceWithIsSetFalse()
    {
        // Act
        var unset = UpdatedBy.Unset();

        // Assert
        Assert.NotNull(unset);
        Assert.False(unset.IsSet);
        Assert.Null(unset.Value);
    }

    /// <summary>TryFrom(null) は true と Unset を返す（null吸収）</summary>
    [Fact]
    public void TryFrom_WithNull_ReturnsTrueAndUnset()
    {
        // Act
        var result = UpdatedBy.TryFrom(null, out var updatedBy);

        // Assert
        Assert.True(result);  // ← 重要：true を返す（失敗ではない）
        Assert.NotNull(updatedBy);
        Assert.False(updatedBy.IsSet);
        Assert.Null(updatedBy.Value);
        Assert.Equal(UpdatedBy.Unset(), updatedBy);
    }

    /// <summary>TryFrom(正の数) は true と インスタンスを返す</summary>
    [Fact]
    public void TryFrom_WithValidValue_ReturnsTrueAndInstance()
    {
        // Act
        var result = UpdatedBy.TryFrom(1002L, out var updatedBy);

        // Assert
        Assert.True(result);
        Assert.NotNull(updatedBy);
        Assert.True(updatedBy.IsSet);
        Assert.Equal(1002L, updatedBy.Value);
    }

    /// <summary>TryFrom(無効値) は false と Unset を返す</summary>
    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    public void TryFrom_WithInvalidValue_ReturnsFalseAndUnset(long invalidValue)
    {
        // Act
        var result = UpdatedBy.TryFrom(invalidValue, out var updatedBy);

        // Assert
        Assert.False(result);
        Assert.NotNull(updatedBy);
        Assert.False(updatedBy.IsSet);
    }

    /// <summary>Unset 同士は等価</summary>
    [Fact]
    public void Equals_WithBothUnset_ReturnsTrue()
    {
        // Arrange
        var unset1 = UpdatedBy.Unset();
        var unset2 = UpdatedBy.Unset();

        // Act & Assert
        Assert.Equal(unset1, unset2);
        Assert.True(unset1 == unset2);
    }

    /// <summary>IsSet が異なる場合は非等価</summary>
    [Fact]
    public void Equals_WithDifferentIsSet_ReturnsFalse()
    {
        // Arrange
        var updated = UpdatedBy.From(1002L);
        var unset = UpdatedBy.Unset();

        // Act & Assert
        Assert.NotEqual(updated, unset);
        Assert.False(updated == unset);
    }

    /// <summary>同じ値で IsSet が同じ場合は等価</summary>
    [Fact]
    public void Equals_WithSameValueAndIsSet_ReturnsTrue()
    {
        // Arrange
        var updated1 = UpdatedBy.From(1002L);
        var updated2 = UpdatedBy.From(1002L);

        // Act & Assert
        Assert.Equal(updated1, updated2);
        Assert.True(updated1 == updated2);
    }

    /// <summary>HasUpdated で更新済み/未更新を判定</summary>
    [Fact]
    public void HasUpdated_ReturnsCorrectState()
    {
        // Arrange
        var updated = UpdatedBy.From(1002L);
        var unset = UpdatedBy.Unset();

        // Act & Assert
        Assert.True(updated.HasUpdated);
        Assert.False(unset.HasUpdated);
    }

    /// <summary>TryFromDbValue で DB NULL が Unset に変換される</summary>
    [Fact]
    public void TryFromDbValue_WithNull_ReturnsTrueAndUnset()
    {
        // Act
        var result = UpdatedBy.TryFromDbValue(null, out var updatedBy);

        // Assert
        Assert.True(result);
        Assert.False(updatedBy.IsSet);
    }

    /// <summary>ToString は IsSet=false で "Unset" を返す</summary>
    [Fact]
    public void ToString_WithUnset_ReturnsUnset()
    {
        // Arrange
        var unset = UpdatedBy.Unset();

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
        var updated = UpdatedBy.From(1002L);

        // Act
        var result = updated.ToString();

        // Assert
        Assert.Equal("1002", result);
    }
}
