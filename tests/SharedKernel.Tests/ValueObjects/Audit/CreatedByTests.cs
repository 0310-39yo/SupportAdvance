namespace SupportAdvance.SharedKernel.Tests.ValueObjects.Audit;

using SupportAdvance.SharedKernel.ValueObjects.Audit;
using Xunit;

/// <summary>
/// CreatedBy（作成者ID） 単体テスト
///
/// テスト観点：
/// - VO-01: From(正の数) は IsSet=true のインスタンスを生成
/// - VO-02: From(0) は ArgumentException を投げる
/// - VO-03: From(負の数) は ArgumentException を投げる
/// - VO-04: TryFrom(null) は false を返す
/// - VO-05: TryFrom(正の数) は true と インスタンスを返す
/// - VO-06: TryFrom(無効値) は false と null を返す
/// - VO-09: 同じ値の2つのインスタンスは等価
/// - VO-10: 異なる値のインスタンスは等価ではない
/// - VO-11: Equals=true のペアは同一ハッシュ値
/// - VO-12: IsSet は常に true
/// </summary>
public class CreatedByTests
{
    /// <summary>VO-01: From(正の数) は IsSet=true のインスタンスを生成</summary>
    [Theory]
    [InlineData(1L)]
    [InlineData(1001L)]
    [InlineData(1000000L)]
    public void From_WithValidPositiveValue_ReturnsInstanceWithIsSetTrue(long validValue)
    {
        // Act
        var createdBy = CreatedBy.From(validValue);

        // Assert
        Assert.NotNull(createdBy);
        Assert.True(createdBy.IsSet);
        Assert.Equal(validValue, createdBy.Value);
    }

    /// <summary>VO-02: From(0) は ArgumentException を投げる</summary>
    [Fact]
    public void From_WithZero_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CreatedBy.From(0L));
    }

    /// <summary>VO-03: From(負の数) は ArgumentException を投げる</summary>
    [Theory]
    [InlineData(-1L)]
    [InlineData(-1000L)]
    [InlineData(long.MinValue)]
    public void From_WithNegativeValue_ThrowsArgumentException(long negativeValue)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CreatedBy.From(negativeValue));
    }

    /// <summary>VO-04: TryFrom(null) は false を返す</summary>
    [Fact]
    public void TryFrom_WithNull_ReturnsFalse()
    {
        // Act
        var result = CreatedBy.TryFrom(null, out var createdBy);

        // Assert
        Assert.False(result);
        Assert.Null(createdBy);
    }

    /// <summary>VO-05: TryFrom(正の数) は true と インスタンスを返す</summary>
    [Theory]
    [InlineData(1L)]
    [InlineData(1001L)]
    public void TryFrom_WithValidValue_ReturnsTrueAndInstance(long validValue)
    {
        // Act
        var result = CreatedBy.TryFrom(validValue, out var createdBy);

        // Assert
        Assert.True(result);
        Assert.NotNull(createdBy);
        Assert.True(createdBy.IsSet);
        Assert.Equal(validValue, createdBy.Value);
    }

    /// <summary>VO-06: TryFrom(無効値) は false と null を返す</summary>
    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    public void TryFrom_WithInvalidValue_ReturnsFalseAndNull(long invalidValue)
    {
        // Act
        var result = CreatedBy.TryFrom(invalidValue, out var createdBy);

        // Assert
        Assert.False(result);
        Assert.Null(createdBy);
    }

    /// <summary>VO-09: 同じ値の2つのインスタンスは等価</summary>
    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        // Arrange
        var createdBy1 = CreatedBy.From(1001L);
        var createdBy2 = CreatedBy.From(1001L);

        // Act & Assert
        Assert.Equal(createdBy1, createdBy2);
        Assert.True(createdBy1 == createdBy2);
        Assert.False(createdBy1 != createdBy2);
    }

    /// <summary>VO-10: 異なる値のインスタンスは等価ではない</summary>
    [Fact]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var createdBy1 = CreatedBy.From(1001L);
        var createdBy2 = CreatedBy.From(1002L);

        // Act & Assert
        Assert.NotEqual(createdBy1, createdBy2);
        Assert.False(createdBy1 == createdBy2);
        Assert.True(createdBy1 != createdBy2);
    }

    /// <summary>VO-11: Equals=true のペアは同一ハッシュ値</summary>
    [Fact]
    public void GetHashCode_WithEqualInstances_ReturnsSameHashCode()
    {
        // Arrange
        var createdBy1 = CreatedBy.From(1001L);
        var createdBy2 = CreatedBy.From(1001L);

        // Act & Assert
        Assert.Equal(createdBy1.GetHashCode(), createdBy2.GetHashCode());

        // HashSet での動作確認
        var set = new HashSet<CreatedBy> { createdBy1, createdBy2 };
        Assert.Single(set);  // 等価なので重複排除される
    }

    /// <summary>VO-12: IsSet は常に true</summary>
    [Fact]
    public void IsSet_AlwaysReturnsTrue()
    {
        // Arrange
        var createdBy = CreatedBy.From(1001L);

        // Act & Assert
        Assert.True(createdBy.IsSet);
    }

    /// <summary>ToString が long の文字列表現を返す</summary>
    [Fact]
    public void ToString_ReturnsStringRepresentationOfValue()
    {
        // Arrange
        var createdBy = CreatedBy.From(1001L);

        // Act
        var result = createdBy.ToString();

        // Assert
        Assert.Equal("1001", result);
    }

    /// <summary>Equals(object) での型チェック</summary>
    [Fact]
    public void Equals_WithDifferentType_ReturnsFalse()
    {
        // Arrange
        var createdBy = CreatedBy.From(1001L);
        var differentObject = "1001";

        // Act & Assert
        Assert.False(createdBy.Equals(differentObject));
    }

    /// <summary>Equals(null) での null チェック</summary>
    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var createdBy = CreatedBy.From(1001L);

        // Act & Assert
        Assert.False(createdBy.Equals(null));
    }
}
