using SupportAdvance.SharedKernel.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.Tests.SharedKernel.Tests.ValueObjects.Fixtures;

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
/// PrimitiveValueObject（スカラ値）のテスト用実装
/// TestStringValue（文字列値）- トリムと正規化をテスト
/// </summary>
public sealed class TestStringValue : PrimitiveValueObject<string>
{
    private TestStringValue(string value, bool isSet) : base(value, isSet) { }

    public static TestStringValue Create(string value, bool isSet = true) => new(value, isSet);
    public static TestStringValue CreateUnset() => new(string.Empty, false);

    public string? Value => IsSet ? ValueField : null;

    /// <summary>
    /// トリム処理を実装
    /// </summary>
    protected override string Normalize(string input) => input.Trim();

    /// <summary>
    /// 空文字列は無効
    /// </summary>
    public override void Validate(string normalized)
    {
        if (string.IsNullOrEmpty(normalized))
            throw new ArgumentException("Value cannot be empty", nameof(normalized));
        if (normalized.Length > 50)
            throw new ArgumentException("Value must be 50 characters or less", nameof(normalized));
    }

    /// <summary>
    /// カスタムフォーマット：値を括弧で囲む
    /// </summary>
    protected override string Format(string value) => $"[{value}]";

    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet) yield return ValueField;
    }
}

/// <summary>
/// PrimitiveValueObject（スカラ値）のテスト用実装
/// TestIntValue（整数値）- 範囲検証をテスト
/// </summary>
public sealed class TestIntValue : PrimitiveValueObject<int>
{
    private TestIntValue(int value, bool isSet) : base(value, isSet) { }

    public static TestIntValue Create(int value, bool isSet = true) => new(value, isSet);
    public static TestIntValue CreateUnset() => new(0, false);

    public int? Value => IsSet ? (int?)ValueField : null;

    /// <summary>
    /// 範囲は 0 ～ 100
    /// </summary>
    public override void Validate(int normalized)
    {
        if (normalized < 0 || normalized > 100)
            throw new ArgumentOutOfRangeException(nameof(normalized), "Value must be between 0 and 100");
    }

    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet) yield return ValueField;
    }
}

/// <summary>
/// PrimitiveValueObject（スカラ値）のテスト用実装
/// TestDecimalValue（小数値）- フォーマット処理をテスト
/// </summary>
public sealed class TestDecimalValue : PrimitiveValueObject<decimal>
{
    private TestDecimalValue(decimal value, bool isSet) : base(value, isSet) { }

    public static TestDecimalValue Create(decimal value, bool isSet = true) => new(value, isSet);
    public static TestDecimalValue CreateUnset() => new(0m, false);

    public decimal? Value => IsSet ? (decimal?)ValueField : null;

    /// <summary>
    /// 負の値は無効
    /// </summary>
    public override void Validate(decimal normalized)
    {
        if (normalized < 0)
            throw new ArgumentException("Value cannot be negative", nameof(normalized));
    }

    /// <summary>
    /// 小数点以下2桁で丸める
    /// </summary>
    protected override decimal Normalize(decimal input) => Math.Round(input, 2);

    /// <summary>
    /// 通貨形式でフォーマット
    /// </summary>
    protected override string Format(decimal value) => $"¥{value:F2}";

    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet) yield return ValueField;
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
