using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.SharedKernel.Tests.ValueObjects.Fixtures;

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

/// <summary>
/// EnumValueObject の具体実装例
/// OrderStatus（注文ステータス：下書き、承認済、完了）
/// </summary>
public sealed class OrderStatus : EnumValueObject<int>
{
    /// <summary>
    /// 下書き（内部値：1）
    /// </summary>
    public static readonly OrderStatus Draft = new(1);

    /// <summary>
    /// 承認済（内部値：2）
    /// </summary>
    public static readonly OrderStatus Approved = new(2);

    /// <summary>
    /// 完了（内部値：3）
    /// </summary>
    public static readonly OrderStatus Completed = new(3);

    /// <summary>
    /// プライベートコンストラクタ - 静的フィールド経由でのみ生成
    /// </summary>
    private OrderStatus(int value)
        : base(value)
    {
    }

    /// <summary>
    /// 内部値から OrderStatus を逆引きする
    /// </summary>
    public static OrderStatus From(int value)
    {
        return value switch
        {
            1 => Draft,
            2 => Approved,
            3 => Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(value), "Invalid order status value")
        };
    }

    /// <summary>
    /// 選択肢の妥当性をチェック（1～3）
    /// </summary>
    protected override void Validate(int value)
    {
        if (value is < 1 or > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Order status must be between 1 and 3");
        }
    }

    /// <summary>
    /// 内部値から業務名称（日本語表示名）を取得
    /// </summary>
    protected override string GetDisplayName()
    {
        return ValueField switch
        {
            1 => "下書き",
            2 => "承認済",
            3 => "完了",
            _ => string.Empty
        };
    }
}
