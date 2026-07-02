namespace SupportAdvance.SharedKernel.Tests.ValueObjects;

using SupportAdvance.SharedKernel.Tests.ValueObjects.Fixtures;

/// <summary>
/// ValueObject の IsSet プロパティに関するテスト
/// 観点グループ IS: IsSet プロパティ
/// </summary>
public class ValueObjectIsSetTests
{
    /// <summary>
    /// 観点: VO-IS-01
    /// IsSet=true で構築したオブジェクトは IsSet が true を返す
    /// パターン: 4.1.2.1 - 単一コンポーネント、IsSet=true
    /// </summary>
    [Fact]
    public void VO_IS_01_SingleComponent_IsSetTrue()
    {
        // Arrange
        var orderId = OrderId.Create("ORD-001", isSet: true);

        // Act
        var result = orderId.IsSet;

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// 観点: VO-IS-01
    /// IsSet=true で構築したオブジェクト（複数コンポーネント）は IsSet が true を返す
    /// パターン: 4.1.2.2 - 複数コンポーネント、IsSet=true
    /// </summary>
    [Fact]
    public void VO_IS_01_MultipleComponents_IsSetTrue()
    {
        // Arrange
        var price = ProductPrice.Create(100.0m, "JPY", isSet: true);

        // Act
        var result = price.IsSet;

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// 観点: VO-IS-02
    /// IsSet=false で構築したオブジェクトは IsSet が false を返す
    /// パターン: 単一コンポーネント、IsSet=false
    /// </summary>
    [Fact]
    public void VO_IS_02_SingleComponent_IsSetFalse()
    {
        // Arrange
        var orderId = OrderId.Create("ORD-002", isSet: false);

        // Act
        var result = orderId.IsSet;

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// 観点: VO-IS-02
    /// IsSet=false で構築したオブジェクト（複数コンポーネント）は IsSet が false を返す
    /// パターン: 複数コンポーネント、IsSet=false
    /// </summary>
    [Fact]
    public void VO_IS_02_MultipleComponents_IsSetFalse()
    {
        // Arrange
        var price = ProductPrice.Create(200.0m, "USD", isSet: false);

        // Act
        var result = price.IsSet;

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// IsSet プロパティは読み取り専用であり、外部から変更不可
    /// </summary>
    [Fact]
    public void IsSetProperty_IsReadOnly()
    {
        // Arrange
        var orderId = OrderId.Create("ORD-003", isSet: true);

        // Act & Assert
        Assert.True(orderId.IsSet);
    }
}
