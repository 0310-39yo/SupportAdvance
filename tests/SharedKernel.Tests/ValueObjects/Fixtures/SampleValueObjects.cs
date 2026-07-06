using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.SharedKernel.Tests.ValueObjects.Fixtures;

/// <summary>
/// 単一コンポーネントを持つサンプルValueObject
/// OrderId（注文ID）
/// </summary>
public class OrderId : ValueObject
{
    public string Value { get; }

    private OrderId(string value, bool isSet = true)
    {
        Value = value;
        IsSet = isSet;
    }

    /// <summary>
    /// OrderId を作成するファクトリメソッド
    /// </summary>
    public static OrderId Create(string value, bool isSet = true) => new(value, isSet);

    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet)
        {
            yield return Value;
        }
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

    private ProductPrice(decimal amount, string currency, bool isSet = true)
    {
        Amount = amount;
        Currency = currency;
        IsSet = isSet;
    }

    /// <summary>
    /// ProductPrice を作成するファクトリメソッド
    /// </summary>
    public static ProductPrice Create(decimal amount, string currency, bool isSet = true)
        => new(amount, currency, isSet);

    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet)
        {
            yield return Amount;
            yield return Currency;
        }
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
    public override void Validate(int value)
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
