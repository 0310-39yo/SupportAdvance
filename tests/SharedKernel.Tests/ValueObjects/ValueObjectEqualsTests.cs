namespace SupportAdvance.SharedKernel.Tests.ValueObjects;

using SupportAdvance.SharedKernel.Tests.ValueObjects.Fixtures;

/// <summary>
/// ValueObject の Equals メソッドに関する等価判定テスト
/// 観点グループ EQ: Equals メソッド - 等価と判定されるケース
/// </summary>
public class ValueObjectEqualsTests
{
    /// <summary>
    /// 観点: VO-EQ-01
    /// 同じ型・同じ値・同じ IsSet を持つ 2 つのオブジェクトは等価
    /// パターン: 4.2.2.1 - 同じ値、同じ IsSet=true
    /// </summary>
    [Fact]
    public void VO_EQ_01_SameValueSameIsSetTrue_AreEqual()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-001", isSet: true);
        var obj2 = OrderId.Create("ORD-001", isSet: true);

        // Act
        var equalsResult = obj1.Equals(obj2);
        var operatorResult = obj1 == obj2;
        var hashCodeEqual = obj1.GetHashCode() == obj2.GetHashCode();

        // Assert
        Assert.True(equalsResult);
        Assert.True(operatorResult);
        Assert.True(hashCodeEqual);
    }

    /// <summary>
    /// 観点: VO-EQ-01
    /// 同じ値・IsSet=false でも等価
    /// パターン: 4.2.2.2 - 同じ値、同じ IsSet=false
    /// </summary>
    [Fact]
    public void VO_EQ_01_SameValueSameIsSetFalse_AreEqual()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-002", isSet: false);
        var obj2 = OrderId.Create("ORD-002", isSet: false);

        // Act
        var equalsResult = obj1.Equals(obj2);
        var operatorResult = obj1 == obj2;

        // Assert
        Assert.True(equalsResult);
        Assert.True(operatorResult);
    }

    /// <summary>
    /// 観点: VO-EQ-01
    /// 複数コンポーネントがすべて一致する場合は等価
    /// パターン: 4.2.2.3 - 複数コンポーネント、すべて一致
    /// </summary>
    [Fact]
    public void VO_EQ_01_MultipleComponentsAllMatch_AreEqual()
    {
        // Arrange
        var obj1 = ProductPrice.Create(100.0m, "JPY", isSet: true);
        var obj2 = ProductPrice.Create(100.0m, "JPY", isSet: true);

        // Act
        var equalsResult = obj1.Equals(obj2);
        var operatorResult = obj1 == obj2;
        var hashCodeEqual = obj1.GetHashCode() == obj2.GetHashCode();

        // Assert
        Assert.True(equalsResult);
        Assert.True(operatorResult);
        Assert.True(hashCodeEqual);
    }

    /// <summary>
    /// 観点: VO-EQ-02
    /// 同一参照のオブジェクトは等価（自己参照比較）
    /// </summary>
    [Fact]
    public void VO_EQ_02_SelfReference_IsEqual()
    {
        // Arrange
        var obj = OrderId.Create("ORD-003", isSet: true);

        // Act
        var equalsResult = obj.Equals(obj);
        var operatorResult = obj == obj;

        // Assert
        Assert.True(equalsResult);
        Assert.True(operatorResult);
    }

    /// <summary>
    /// 観点: VO-EQ-03
    /// 複数コンポーネントのすべてが一致する場合は等価
    /// </summary>
    [Fact]
    public void VO_EQ_03_MultipleComponentsPartialMatch_AreEqual()
    {
        // Arrange
        var obj1 = ProductPrice.Create(150.5m, "EUR", isSet: true);
        var obj2 = ProductPrice.Create(150.5m, "EUR", isSet: true);

        // Act
        var equalsResult = obj1.Equals(obj2);

        // Assert
        Assert.True(equalsResult);
    }

    /// <summary>
    /// 観点: VO-EQ-04
    /// 両方が IsSet=false で値が同じ場合は等価
    /// </summary>
    [Fact]
    public void VO_EQ_04_BothUnsetSameValue_AreEqual()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-004", isSet: false);
        var obj2 = OrderId.Create("ORD-004", isSet: false);

        // Act
        var equalsResult = obj1.Equals(obj2);
        var hashCodeEqual = obj1.GetHashCode() == obj2.GetHashCode();

        // Assert
        Assert.True(equalsResult);
        Assert.True(hashCodeEqual);
    }

    /// <summary>
    /// Equals メソッドは複数回呼び出しても一貫性がある
    /// </summary>
    [Fact]
    public void Equals_MultipleCalls_Consistent()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-005", isSet: true);
        var obj2 = OrderId.Create("ORD-005", isSet: true);

        // Act & Assert
        Assert.True(obj1.Equals(obj2));
        Assert.True(obj1.Equals(obj2));
        Assert.True(obj1.Equals(obj2));
    }
}
