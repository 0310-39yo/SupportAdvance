using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using Xunit;

namespace SupportAdvance.Contexts.Employee.Domain.Tests.ValueObjects.Employee;

/// <summary>
/// BizDivision ValueObject の単体テスト
/// 【責務】従業員種別区分（M:従業員、T:派遣社員、C:請負者）を管理
/// 【テスト対象】BizDivision のコンストラクタ、ファクトリメソッド、プロパティ
/// </summary>
public class BizDivisionTests
{
    /// <summary>
    /// RegularEmployee() で従業員区分が生成されることを検証
    /// </summary>
    [Fact]
    public void RegularEmployee_CreatesEmployeeInstance()
    {
        // Act
        var bizDivision = BizDivision.RegularEmployee();

        // Assert
        Assert.NotNull(bizDivision);
        Assert.Equal('M', bizDivision.Value);
    }

    /// <summary>
    /// Dispatched() で派遣社員区分が生成されることを検証
    /// </summary>
    [Fact]
    public void Dispatched_CreatesDispatchedInstance()
    {
        // Act
        var bizDivision = BizDivision.Dispatched();

        // Assert
        Assert.NotNull(bizDivision);
        Assert.Equal('T', bizDivision.Value);
    }

    /// <summary>
    /// Contractor() で請負者区分が生成されることを検証
    /// </summary>
    [Fact]
    public void Contractor_CreatesContractorInstance()
    {
        // Act
        var bizDivision = BizDivision.Contractor();

        // Assert
        Assert.NotNull(bizDivision);
        Assert.Equal('C', bizDivision.Value);
    }

    /// <summary>
    /// From() で指定した char 値から BizDivision が生成されることを検証
    /// </summary>
    [Fact]
    public void From_CreatesBizDivisionFromChar()
    {
        // Act
        var bizDivision = BizDivision.From('M');

        // Assert
        Assert.NotNull(bizDivision);
        Assert.Equal('M', bizDivision.Value);
    }

    /// <summary>
    /// TryFrom() で有効な char 値が true を返すことを検証
    /// </summary>
    [Fact]
    public void TryFrom_WithValidChar_ReturnsTrue()
    {
        // Act
        var result = BizDivision.TryFrom('T', out var bizDivision);

        // Assert
        Assert.True(result);
        Assert.NotNull(bizDivision);
        Assert.Equal('T', bizDivision.Value);
    }

    /// <summary>
    /// TryFrom() で null を指定すると false を返すことを検証
    /// </summary>
    [Fact]
    public void TryFrom_WithNullableNull_ReturnsFalse()
    {
        // Act
        var result = BizDivision.TryFrom(null, out var bizDivision);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// FromDbValue() で DB 値から BizDivision が生成されることを検証
    /// </summary>
    [Fact]
    public void FromDbValue_CreatesFromDatabaseValue()
    {
        // Act
        var bizDivision = BizDivision.FromDbValue("M");

        // Assert
        Assert.NotNull(bizDivision);
        Assert.Equal('M', bizDivision.Value);
    }

    /// <summary>
    /// FromDbValue() で空文字を指定するとエラーを投出することを検証
    /// </summary>
    [Fact]
    public void FromDbValue_WithEmpty_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => BizDivision.FromDbValue(""));
        Assert.Contains("value cannot be empty", exception.Message);
    }

    /// <summary>
    /// 等値比較が正しく機能することを検証
    /// </summary>
    [Fact]
    public void Equality_WorksCorrectly()
    {
        // Arrange
        var div1 = BizDivision.RegularEmployee();
        var div2 = BizDivision.RegularEmployee();
        var div3 = BizDivision.Dispatched();

        // Act & Assert
        Assert.Equal(div1, div2);
        Assert.NotEqual(div1, div3);
    }

    /// <summary>
    /// BizDivision が ValueObject を継承していることを検証
    /// </summary>
    [Fact]
    public void BizDivision_InheritsValueObject()
    {
        // Act
        var bizDivision = BizDivision.RegularEmployee();

        // Assert
        Assert.IsAssignableFrom<SupportAdvance.SharedKernel.ValueObjects.ValueObject>(bizDivision);
    }
}
