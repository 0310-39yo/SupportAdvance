namespace SupportAdvance.SharedKernel.Tests.ValueObjects.Fixtures;

using SupportAdvance.SharedKernel.ValueObjects;
using System.Reflection;

/// <summary>
/// ValueObject の protected init プロパティを設定するヘルパー
/// </summary>
internal static class ValueObjectHelper
{
    /// <summary>
    /// リフレクションを使用して ValueObject の IsSet プロパティを設定する
    /// </summary>
    internal static void SetIsSet(ValueObject obj, bool isSet)
    {
        var field = typeof(ValueObject).GetField(
            "<IsSet>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (field != null)
        {
            field.SetValue(obj, isSet);
        }
    }
}

/// <summary>
/// 単一コンポーネントを持つサンプルValueObject
/// OrderId（注文ID）
/// </summary>
public class OrderId : ValueObject
{
    public string Value { get; }

    private OrderId(string value)
    {
        Value = value;
    }

    /// <summary>
    /// OrderId を作成するファクトリメソッド
    /// IsSet の状態を指定できる
    /// </summary>
    public static OrderId Create(string value, bool isSet = true)
    {
        var result = new OrderId(value);
        ValueObjectHelper.SetIsSet(result, isSet);
        return result;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}

/// <summary>
/// 複数コンポーネントを持つサンプルValueObject
/// ProductPrice（商品価格：金額と通貨）
/// </summary>
public class ProductPrice : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private ProductPrice(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// ProductPrice を作成するファクトリメソッド
    /// IsSet の状態を指定できる
    /// </summary>
    public static ProductPrice Create(decimal amount, string currency, bool isSet = true)
    {
        var result = new ProductPrice(amount, currency);
        ValueObjectHelper.SetIsSet(result, isSet);
        return result;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
