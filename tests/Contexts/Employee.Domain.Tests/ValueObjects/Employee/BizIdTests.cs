using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using Xunit;

namespace SupportAdvance.Contexts.Employee.Domain.Tests.ValueObjects.Employee;

/// <summary>
/// BizId ValueObject の単体テスト
/// 【責務】従業員の通し番号（ビジネス ID、1001以上）を管理
/// 【テスト対象】BizId のコンストラクタ、ファクトリメソッド、検証ロジック
/// </summary>
public class BizIdTests
{
    /// <summary>
    /// From() で有効な値から BizId が生成されることを検証
    /// </summary>
    [Fact]
    public void From_WithValidValue_CreatesBizId()
    {
        // Act
        var bizId = BizId.From(1001);

        // Assert
        Assert.NotNull(bizId);
        Assert.Equal(1001, bizId.Value);
    }

    /// <summary>
    /// From() で大きな値も受け入れ可能であることを検証
    /// </summary>
    [Fact]
    public void From_WithLargeValue_CreatesBizId()
    {
        // Act
        var bizId = BizId.From(999999);

        // Assert
        Assert.NotNull(bizId);
        Assert.Equal(999999, bizId.Value);
    }

    /// <summary>
    /// TryFrom() で有効な値が true を返すことを検証
    /// </summary>
    [Fact]
    public void TryFrom_WithValidValue_ReturnsTrue()
    {
        // Act
        var result = BizId.TryFrom(1500, out var bizId);

        // Assert
        Assert.True(result);
        Assert.NotNull(bizId);
        Assert.Equal(1500, bizId.Value);
    }

    /// <summary>
    /// TryFrom() で null を指定すると false を返すことを検証
    /// </summary>
    [Fact]
    public void TryFrom_WithNull_ReturnsFalse()
    {
        // Act
        var result = BizId.TryFrom(null, out var bizId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// FromDbValue() で DB 値から BizId が生成されることを検証
    /// </summary>
    [Fact]
    public void FromDbValue_CreatesFromDatabaseValue()
    {
        // Act
        var bizId = BizId.FromDbValue(2000);

        // Assert
        Assert.NotNull(bizId);
        Assert.Equal(2000, bizId.Value);
    }

    /// <summary>
    /// TryFromDbValue() で有効な DB 値が true を返すことを検証
    /// </summary>
    [Fact]
    public void TryFromDbValue_WithValidValue_ReturnsTrue()
    {
        // Act
        var result = BizId.TryFromDbValue(3000, out var bizId);

        // Assert
        Assert.True(result);
        Assert.NotNull(bizId);
        Assert.Equal(3000, bizId.Value);
    }

    /// <summary>
    /// TryFromDbValue() で null を指定すると false を返すことを検証
    /// </summary>
    [Fact]
    public void TryFromDbValue_WithNull_ReturnsFalse()
    {
        // Act
        var result = BizId.TryFromDbValue(null, out var bizId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// MinValue 定数が正しい値であることを検証
    /// </summary>
    [Fact]
    public void MinValue_IsCorrectValue()
    {
        // Act & Assert
        Assert.Equal(1001, BizId.MinValue);
    }

    /// <summary>
    /// ReservedValue 定数が正しい値であることを検証
    /// </summary>
    [Fact]
    public void ReservedValue_IsCorrectValue()
    {
        // Act & Assert
        Assert.Equal(1000, BizId.ReservedValue);
    }

    /// <summary>
    /// 等値比較が正しく機能することを検証
    /// </summary>
    [Fact]
    public void Equality_WorksCorrectly()
    {
        // Arrange
        var bizId1 = BizId.From(1001);
        var bizId2 = BizId.From(1001);
        var bizId3 = BizId.From(1002);

        // Act & Assert
        Assert.Equal(bizId1, bizId2);
        Assert.NotEqual(bizId1, bizId3);
    }

    /// <summary>
    /// BizId が PrimitiveValueObject を継承していることを検証
    /// </summary>
    [Fact]
    public void BizId_InheritsPrimitiveValueObject()
    {
        // Act
        var bizId = BizId.From(1001);

        // Assert
        Assert.IsAssignableFrom<SupportAdvance.SharedKernel.ValueObjects.Abstractions.PrimitiveValueObject<int>>(bizId);
    }
}
