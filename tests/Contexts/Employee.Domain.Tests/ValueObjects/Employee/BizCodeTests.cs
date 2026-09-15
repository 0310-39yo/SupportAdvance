using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using Xunit;

namespace SupportAdvance.Contexts.Employee.Domain.Tests.ValueObjects.Employee;

/// <summary>
/// BizCode ValueObject の単体テスト
/// 【責務】従業員コード（BizDivision + BizId）をコンポジットで管理
/// 【テスト対象】BizCode のファクトリメソッド、パース、表示形式
/// </summary>
public class BizCodeTests
{
    /// <summary>
    /// From() で BizDivision と BizId から BizCode が生成されることを検証
    /// </summary>
    [Fact]
    public void From_WithValidComponents_CreatesBizCode()
    {
        // Arrange
        var division = BizDivision.RegularEmployee();
        var bizId = BizId.From(1001);

        // Act
        var bizCode = BizCode.From(division, bizId);

        // Assert
        Assert.NotNull(bizCode);
        Assert.Equal(division, bizCode.Division);
        Assert.Equal(bizId, bizCode.BizId);
    }

    /// <summary>
    /// From() で複数の従業員種別でも生成できることを検証
    /// </summary>
    [Fact]
    public void From_WithDispatchedDivision_CreatesBizCode()
    {
        // Arrange
        var division = BizDivision.Dispatched();
        var bizId = BizId.From(7500);

        // Act
        var bizCode = BizCode.From(division, bizId);

        // Assert
        Assert.NotNull(bizCode);
        Assert.Equal('T', bizCode.Division.Value);
    }

    /// <summary>
    /// TryFrom() で有効な値が true を返すことを検証
    /// </summary>
    [Fact]
    public void TryFrom_WithValidComponents_ReturnsTrue()
    {
        // Arrange
        var division = BizDivision.RegularEmployee();
        var bizId = BizId.From(1500);

        // Act
        var result = BizCode.TryFrom(division, bizId, out var bizCode);

        // Assert
        Assert.True(result);
        Assert.NotNull(bizCode);
    }

    /// <summary>
    /// TryFrom() で null division が false を返すことを検証
    /// </summary>
    [Fact]
    public void TryFrom_WithNullDivision_ReturnsFalse()
    {
        // Arrange
        var bizId = BizId.From(1500);

        // Act
        var result = BizCode.TryFrom(null!, bizId, out var bizCode);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// TryFrom() で null bizId が false を返すことを検証
    /// </summary>
    [Fact]
    public void TryFrom_WithNullBizId_ReturnsFalse()
    {
        // Arrange
        var division = BizDivision.RegularEmployee();

        // Act
        var result = BizCode.TryFrom(division, null!, out var bizCode);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// TryParse() で有効な文字列をパースできることを検証
    /// </summary>
    [Fact]
    public void TryParse_WithValidFormat_ReturnsTrueAndParsesBizCode()
    {
        // Act
        var result = BizCode.TryParse("M1001", out var bizCode);

        // Assert
        Assert.True(result);
        Assert.NotNull(bizCode);
        Assert.Equal('M', bizCode.Division.Value);
        Assert.Equal(1001, bizCode.BizId.Value);
    }

    /// <summary>
    /// TryParse() で異なる従業員種別もパースできることを検証
    /// </summary>
    [Fact]
    public void TryParse_WithDispatchedCode_ReturnsTrueAndParsesCorrectly()
    {
        // Act
        var result = BizCode.TryParse("T7500", out var bizCode);

        // Assert
        Assert.True(result);
        Assert.NotNull(bizCode);
        Assert.Equal('T', bizCode.Division.Value);
        Assert.Equal(7500, bizCode.BizId.Value);
    }

    /// <summary>
    /// TryParse() で null または空文字列が false を返すことを検証
    /// </summary>
    [Fact]
    public void TryParse_WithNullOrEmpty_ReturnsFalse()
    {
        // Act
        var result1 = BizCode.TryParse(null, out var bizCode1);
        var result2 = BizCode.TryParse(string.Empty, out var bizCode2);

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }

    /// <summary>
    /// 等値比較が正しく機能することを検証
    /// </summary>
    [Fact]
    public void Equality_WorksCorrectly()
    {
        // Arrange
        var bizCode1 = BizCode.From(BizDivision.RegularEmployee(), BizId.From(1001));
        var bizCode2 = BizCode.From(BizDivision.RegularEmployee(), BizId.From(1001));
        var bizCode3 = BizCode.From(BizDivision.RegularEmployee(), BizId.From(1002));

        // Act & Assert
        Assert.Equal(bizCode1, bizCode2);
        Assert.NotEqual(bizCode1, bizCode3);
    }

    /// <summary>
    /// BizCode が ValueObject を継承していることを検証
    /// </summary>
    [Fact]
    public void BizCode_InheritsValueObject()
    {
        // Act
        var bizCode = BizCode.From(BizDivision.RegularEmployee(), BizId.From(1001));

        // Assert
        Assert.IsAssignableFrom<SupportAdvance.SharedKernel.ValueObjects.ValueObject>(bizCode);
    }
}
