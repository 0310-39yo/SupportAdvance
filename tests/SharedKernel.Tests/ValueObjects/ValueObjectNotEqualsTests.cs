namespace SupportAdvance.SharedKernel.Tests.ValueObjects;

using SupportAdvance.SharedKernel.Tests.ValueObjects.Fixtures;

/// <summary>
/// ValueObject の Equals メソッドに関する非等価判定テスト
/// 観点グループ NE: Equals メソッド - 非等価と判定されるケース
/// </summary>
public class ValueObjectNotEqualsTests
{
    /// <summary>
    /// 観点: VO-NE-01
    /// コンポーネント値が異なる場合は非等価
    /// </summary>
    [Fact]
    public void VO_NE_01_DifferentComponentValue_AreNotEqual()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-001", isSet: true);
        var obj2 = OrderId.Create("ORD-002", isSet: true);

        // Act
        var equalsResult = obj1.Equals(obj2);
        var operatorResult = obj1 == obj2;
        var hashCodeEqual = obj1.GetHashCode() == obj2.GetHashCode();

        // Assert
        Assert.False(equalsResult);
        Assert.False(operatorResult);
        Assert.False(hashCodeEqual);
    }

    /// <summary>
    /// 観点: VO-NE-02
    /// IsSet フラグが異なる場合は非等価（値が同じでも）
    /// パターン: 4.3.2.1 - IsSet: true vs false
    /// </summary>
    [Fact]
    public void VO_NE_02_DifferentIsSetFlag_TrueVsFalse_AreNotEqual()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-003", isSet: true);
        var obj2 = OrderId.Create("ORD-003", isSet: false);

        // Act
        var equalsResult = obj1.Equals(obj2);
        var operatorResult = obj1 == obj2;
        var hashCodeEqual = obj1.GetHashCode() == obj2.GetHashCode();

        // Assert
        Assert.False(equalsResult);
        Assert.False(operatorResult);
        Assert.False(hashCodeEqual);
    }

    /// <summary>
    /// 観点: VO-NE-02
    /// IsSet フラグが異なる場合は非等価（逆順）
    /// パターン: 4.3.2.2 - IsSet: false vs true（逆順）
    /// </summary>
    [Fact]
    public void VO_NE_02_DifferentIsSetFlag_FalseVsTrue_AreNotEqual()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-004", isSet: false);
        var obj2 = OrderId.Create("ORD-004", isSet: true);

        // Act
        var equalsResult = obj1.Equals(obj2);
        var operatorResult = obj1 == obj2;

        // Assert
        Assert.False(equalsResult);
        Assert.False(operatorResult);
    }

    /// <summary>
    /// 観点: VO-NE-03
    /// 型が異なる場合は非等価（値が同じでも）
    /// </summary>
    [Fact]
    public void VO_NE_03_DifferentType_AreNotEqual()
    {
        // Arrange
        var orderObj = OrderId.Create("100", isSet: true);
        var priceObj = ProductPrice.Create(100.0m, "JPY", isSet: true);

        // Act
        var equalsResult = orderObj.Equals(priceObj);
        var operatorResult = orderObj == priceObj;

        // Assert
        Assert.False(equalsResult);
        Assert.False(operatorResult);
    }

    /// <summary>
    /// 観点: VO-NE-04
    /// null との比較は非等価
    /// </summary>
    [Fact]
    public void VO_NE_04_CompareWithNull_AreNotEqual()
    {
        // Arrange
        var obj = OrderId.Create("ORD-005", isSet: true);
        OrderId? nullObj = null;

        // Act
        var equalsResult = obj.Equals(nullObj);
        var operatorResult = obj == nullObj;
        var operatorNotEqualResult = obj != nullObj;

        // Assert
        Assert.False(equalsResult);
        Assert.False(operatorResult);
        Assert.True(operatorNotEqualResult);
    }

    /// <summary>
    /// 観点: VO-NE-05
    /// 複数コンポーネントの一部が異なる場合は非等価
    /// </summary>
    [Fact]
    public void VO_NE_05_MultipleComponentsPartialDifference_AreNotEqual()
    {
        // Arrange
        var obj1 = ProductPrice.Create(100.0m, "JPY", isSet: true);
        var obj2 = ProductPrice.Create(100.0m, "USD", isSet: true);  // Currency が異なる

        // Act
        var equalsResult = obj1.Equals(obj2);
        var operatorResult = obj1 == obj2;
        var hashCodeEqual = obj1.GetHashCode() == obj2.GetHashCode();

        // Assert
        Assert.False(equalsResult);
        Assert.False(operatorResult);
        Assert.False(hashCodeEqual);
    }

    /// <summary>
    /// 複数コンポーネントの別の一部が異なる場合も非等価
    /// </summary>
    [Fact]
    public void MultipleComponentsPartialDifference_AmountDifferent_AreNotEqual()
    {
        // Arrange
        var obj1 = ProductPrice.Create(100.0m, "JPY", isSet: true);
        var obj2 = ProductPrice.Create(150.0m, "JPY", isSet: true);  // Amount が異なる

        // Act
        var equalsResult = obj1.Equals(obj2);

        // Assert
        Assert.False(equalsResult);
    }

    /// <summary>
    /// 相互性の検証：obj1.Equals(obj2) == obj2.Equals(obj1)
    /// </summary>
    [Fact]
    public void Equals_IsSymmetric()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-006", isSet: true);
        var obj2 = OrderId.Create("ORD-007", isSet: true);

        // Act
        var result1 = obj1.Equals(obj2);
        var result2 = obj2.Equals(obj1);

        // Assert
        Assert.Equal(result1, result2);
    }

    /// <summary>
    /// 推移性の検証：obj1 == obj2 && obj2 == obj3 ⇒ obj1 == obj3
    /// </summary>
    [Fact]
    public void Equals_IsTransitive()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-008", isSet: true);
        var obj2 = OrderId.Create("ORD-008", isSet: true);
        var obj3 = OrderId.Create("ORD-008", isSet: true);

        // Act
        var result12 = obj1.Equals(obj2);
        var result23 = obj2.Equals(obj3);
        var result13 = obj1.Equals(obj3);

        // Assert
        Assert.True(result12);
        Assert.True(result23);
        Assert.True(result13);
    }
}
