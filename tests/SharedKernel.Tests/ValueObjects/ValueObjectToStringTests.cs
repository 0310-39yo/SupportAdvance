namespace SupportAdvance.SharedKernel.Tests.ValueObjects;

using SupportAdvance.SharedKernel.Tests.ValueObjects.Fixtures;

/// <summary>
/// ValueObject の ToString メソッドに関するテスト
/// 観点グループ TS: ToString メソッド
/// </summary>
public class ValueObjectToStringTests
{
    /// <summary>
    /// 観点: VO-TS-01
    /// IsSet=false のとき "Unset" を返す
    /// パターン: 4.5.2.1
    /// </summary>
    [Fact]
    public void VO_TS_01_IsSetFalse_ReturnsUnset()
    {
        // Arrange
        var obj = OrderId.Create("ORD-001", isSet: false);

        // Act
        var result = obj.ToString();

        // Assert
        Assert.Equal("Unset", result);
    }

    /// <summary>
    /// 観点: VO-TS-01
    /// IsSet=false の複数コンポーネントオブジェクトでも "Unset" を返す
    /// </summary>
    [Fact]
    public void VO_TS_01_MultipleComponentsIsSetFalse_ReturnsUnset()
    {
        // Arrange
        var obj = ProductPrice.Create(100.0m, "JPY", isSet: false);

        // Act
        var result = obj.ToString();

        // Assert
        Assert.Equal("Unset", result);
    }

    /// <summary>
    /// 観点: VO-TS-02
    /// IsSet=true のとき、コンポーネントをカンマ区切りで連結して返す
    /// パターン: 4.5.2.2 - 単一コンポーネント
    /// </summary>
    [Fact]
    public void VO_TS_02_IsSetTrue_SingleComponent_ReturnsCombinedValue()
    {
        // Arrange
        var obj = OrderId.Create("ORD-001", isSet: true);

        // Act
        var result = obj.ToString();

        // Assert
        Assert.Equal("ORD-001", result);
    }

    /// <summary>
    /// 観点: VO-TS-02
    /// IsSet=true のとき、複数コンポーネントをカンマで区切って連結
    /// パターン: 4.5.2.2 - 複数コンポーネント
    /// </summary>
    [Fact]
    public void VO_TS_02_IsSetTrue_MultipleComponents_ReturnsCombinedValue()
    {
        // Arrange
        var obj = ProductPrice.Create(100.0m, "JPY", isSet: true);

        // Act
        var result = obj.ToString();

        // Assert
        // 注: decimal(100m)は"100"と表示されるが、decimal(100.0m)は"100.0"と表示される
        Assert.Equal("100.0, JPY", result);
    }

    /// <summary>
    /// 観点: VO-TS-02
    /// IsSet=true のとき、複数値がカンマで区切られている
    /// </summary>
    [Fact]
    public void VO_TS_02_MultipleComponents_CommaDelimited()
    {
        // Arrange
        var obj = ProductPrice.Create(250.5m, "USD", isSet: true);

        // Act
        var result = obj.ToString();

        // Assert
        Assert.Contains(", ", result);
        Assert.Equal("250.5, USD", result);
    }

    /// <summary>
    /// 観点: VO-TS-03
    /// ToString 出力に IsSet 値自体が含まれない
    /// </summary>
    [Fact]
    public void VO_TS_03_IsSetNotInOutput()
    {
        // Arrange
        var objTrue = OrderId.Create("ORD-002", isSet: true);
        var objFalse = OrderId.Create("ORD-003", isSet: false);

        // Act
        var resultTrue = objTrue.ToString();
        var resultFalse = objFalse.ToString();

        // Assert
        Assert.DoesNotContain("True", resultTrue);
        Assert.DoesNotContain("False", resultTrue);
        Assert.DoesNotContain("True", resultFalse);
        Assert.DoesNotContain("False", resultFalse);
    }

    /// <summary>
    /// 観点: VO-TS-03
    /// ToString では IsSet フラグ（bool値）そのものは含まれない
    /// 単にコンポーネント値のみ
    /// </summary>
    [Fact]
    public void VO_TS_03_ComponentValueOnly()
    {
        // Arrange
        var obj1 = OrderId.Create("ORD-004", isSet: true);
        var obj2 = OrderId.Create("ORD-004", isSet: false);

        // Act
        var resultTrue = obj1.ToString();
        var resultFalse = obj2.ToString();

        // Assert
        Assert.Equal("ORD-004", resultTrue);
        Assert.Equal("Unset", resultFalse);
        Assert.NotEqual("True, ORD-004", resultTrue);
    }

    /// <summary>
    /// 複数の異なるコンポーネントを検証
    /// </summary>
    [Fact]
    public void ToString_VariousComponentValues()
    {
        // Arrange & Act & Assert
        var obj1Result = ProductPrice.Create(0m, "JPY", isSet: true).ToString();
        Assert.Equal("0, JPY", obj1Result);

        var obj2Result = ProductPrice.Create(999999.99m, "EUR", isSet: true).ToString();
        Assert.Equal("999999.99, EUR", obj2Result);

        var obj3Result = OrderId.Create("TEST-123", isSet: true).ToString();
        Assert.Equal("TEST-123", obj3Result);
    }

    /// <summary>
    /// IsSet の値に応じた動作の検証
    /// </summary>
    [Fact]
    public void ToString_IsSetDependentBehavior()
    {
        // Arrange
        var unsetObj = OrderId.Create("ORD-005", isSet: false);
        var setObj = OrderId.Create("ORD-005", isSet: true);

        // Act
        var unsetResult = unsetObj.ToString();
        var setResult = setObj.ToString();

        // Assert
        Assert.Equal("Unset", unsetResult);
        Assert.Equal("ORD-005", setResult);
        Assert.NotEqual(unsetResult, setResult);
    }
}
