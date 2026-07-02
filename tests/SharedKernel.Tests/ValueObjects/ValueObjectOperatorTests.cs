namespace SupportAdvance.SharedKernel.Tests.ValueObjects;

using SupportAdvance.SharedKernel.Tests.ValueObjects.Fixtures;

/// <summary>
/// ValueObject の == 演算子と != 演算子に関するテスト
/// 観点グループ OP: == / != 演算子
/// </summary>
public class ValueObjectOperatorTests
{
    /// <summary>
    /// 観点: VO-OP-01
    /// 等価なオブジェクトに == を適用すると true
    /// </summary>
    [Fact]
    public void VO_OP_01_EqualObjects_OperatorEqualReturnsTrue()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-001", isSet: true);
        var obj2 = OrderId.Create("ORD-001", isSet: true);

        // Act
        var result = obj1 == obj2;

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// 観点: VO-OP-02
    /// 非等価なオブジェクトに == を適用すると false
    /// </summary>
    [Fact]
    public void VO_OP_02_NotEqualObjects_OperatorEqualReturnsFalse()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-001", isSet: true);
        var obj2 = OrderId.Create("ORD-002", isSet: true);

        // Act
        var result = obj1 == obj2;

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// 観点: VO-OP-03
    /// 両辺が null のとき == は true
    /// </summary>
    [Fact]
    public void VO_OP_03_BothNull_OperatorEqualReturnsTrue()
    {
        // Arrange
        OrderId? obj1 = null;
        OrderId? obj2 = null;

        // Act
        var result = obj1 == obj2;

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// 観点: VO-OP-04
    /// 片方のみが null のとき == は false
    /// </summary>
    [Fact]
    public void VO_OP_04_OneNull_OperatorEqualReturnsFalse()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-003", isSet: true);
        OrderId? obj2 = null;

        // Act
        var result1 = obj1 == obj2;
        var result2 = obj2 == obj1;

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }

    /// <summary>
    /// 観点: VO-OP-05
    /// != は == の否定と一致する
    /// </summary>
    [Fact]
    public void VO_OP_05_NotEqualOperator_IsNegationOfEqual()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-004", isSet: true);
        var obj2 = OrderId.Create("ORD-005", isSet: true);

        // Act
        var equalResult = obj1 == obj2;
        var notEqualResult = obj1 != obj2;

        // Assert
        Assert.NotEqual(equalResult, notEqualResult);
        Assert.True(notEqualResult);
    }

    /// <summary>
    /// 観点: VO-OP-05
    /// != は == の否定と一致する（等価な場合）
    /// </summary>
    [Fact]
    public void VO_OP_05_NotEqualOperator_IsNegationOfEqual_WhenEqual()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-006", isSet: true);
        var obj2 = OrderId.Create("ORD-006", isSet: true);

        // Act
        var equalResult = obj1 == obj2;
        var notEqualResult = obj1 != obj2;

        // Assert
        Assert.NotEqual(equalResult, notEqualResult);
        Assert.False(notEqualResult);
    }

    /// <summary>
    /// 観点: VO-OP-05
    /// 両方が null のとき != は false
    /// </summary>
    [Fact]
    public void VO_OP_05_NotEqualOperator_BothNull_ReturnsFalse()
    {
        // Arrange
        OrderId? obj1 = null;
        OrderId? obj2 = null;

        // Act
        var result = obj1 != obj2;

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// 観点: VO-OP-05
    /// 片方が null のとき != は true
    /// </summary>
    [Fact]
    public void VO_OP_05_NotEqualOperator_OneNull_ReturnsTrue()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-007", isSet: true);
        OrderId? obj2 = null;

        // Act
        var result = obj1 != obj2;

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// == と Equals メソッドの結果が一致する
    /// </summary>
    [Fact]
    public void OperatorEqual_ConsistentWithEquals()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-008", isSet: true);
        var obj2 = OrderId.Create("ORD-008", isSet: true);

        // Act
        var operatorResult = obj1 == obj2;
        var equalsResult = obj1.Equals(obj2);

        // Assert
        Assert.Equal(operatorResult, equalsResult);
    }

    /// <summary>
    /// == と Equals メソッドの結果が一致する（非等価な場合）
    /// </summary>
    [Fact]
    public void OperatorEqual_ConsistentWithEquals_NotEqual()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-009", isSet: true);
        var obj2 = OrderId.Create("ORD-010", isSet: true);

        // Act
        var operatorResult = obj1 == obj2;
        var equalsResult = obj1.Equals(obj2);

        // Assert
        Assert.Equal(operatorResult, equalsResult);
    }

    /// <summary>
    /// 複数コンポーネントでの == 演算子の動作
    /// </summary>
    [Fact]
    public void OperatorEqual_MultipleComponents()
    {
        // Arrange
        var obj1 = ProductPrice.Create(100.0m, "JPY", isSet: true);
        var obj2 = ProductPrice.Create(100.0m, "JPY", isSet: true);
        var obj3 = ProductPrice.Create(150.0m, "JPY", isSet: true);

        // Act
        var result12 = obj1 == obj2;
        var result13 = obj1 == obj3;

        // Assert
        Assert.True(result12);
        Assert.False(result13);
    }
}
