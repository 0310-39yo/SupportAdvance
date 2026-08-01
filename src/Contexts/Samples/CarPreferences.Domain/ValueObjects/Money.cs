namespace SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;

/// <summary>
/// 金額を表す ValueObject
///
/// 【責務】金額の妥当性検証と等価性判定
/// 【不変性】一度作成されたら変更不可
/// 【検証】非負の値のみ許可
/// 【通貨】予算管理なので JPN を想定（拡張時に Currency を追加）
/// </summary>
public sealed class Money : IEquatable<Money>, IComparable<Money>
{
    /// <summary>
    /// 金額の値（円）
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// 最小値（0円）
    /// </summary>
    public static readonly Money Zero = new(0);

    /// <summary>
    /// 最大値（1000万円）
    /// </summary>
    public static readonly Money Max = new(10_000_000);

    private Money(decimal amount)
    {
        Amount = amount;
    }

    /// <summary>
    /// 金額を生成
    ///
    /// 【検証】
    /// - amount が負数の場合は ArgumentException
    /// - amount が 1000万円を超える場合は ArgumentException
    /// </summary>
    /// <param name="amount">金額（円）</param>
    /// <returns>Money インスタンス</returns>
    /// <exception cref="ArgumentException">値が無効な場合</exception>
    public static Money From(decimal amount)
    {
        if (amount < 0)
        {
            throw new ArgumentException(
                "Money amount must be non-negative",
                nameof(amount));
        }

        if (amount > Max.Amount)
        {
            throw new ArgumentException(
                $"Money amount must not exceed {Max.Amount:N0}",
                nameof(amount));
        }

        return new Money(amount);
    }

    /// <summary>
    /// 金額の生成を試みる（nullable 版）
    ///
    /// 【用途】外部入力の安全な処理
    /// </summary>
    public static bool TryFrom(decimal? amount, out Money result)
    {
        result = Zero;
        if (!amount.HasValue)
        {
            return false;
        }

        if (amount < 0 || amount > Max.Amount)
        {
            return false;
        }

        result = new Money(amount.Value);
        return true;
    }

    /// <summary>
    /// 等価性判定（値ベース）
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as Money);

    /// <summary>
    /// 等価性判定（型安全版）
    /// </summary>
    public bool Equals(Money? other) => other != null && Amount == other.Amount;

    /// <summary>
    /// ハッシュコード取得
    /// </summary>
    public override int GetHashCode() => Amount.GetHashCode();

    /// <summary>
    /// 文字列表現（フォーマット: ¥1,234,567）
    /// </summary>
    public override string ToString() => Amount.ToString("C0", System.Globalization.CultureInfo.GetCultureInfo("ja-JP"));

    /// <summary>
    /// 大小比較
    /// </summary>
    public int CompareTo(Money? other)
    {
        if (other is null) return 1;
        return Amount.CompareTo(other.Amount);
    }

    /// <summary>
    /// 加算
    /// </summary>
    public static Money operator +(Money left, Money right)
    {
        if (left is null || right is null)
        {
            throw new ArgumentNullException(left is null ? nameof(left) : nameof(right));
        }

        return From(left.Amount + right.Amount);
    }

    /// <summary>
    /// 減算
    /// </summary>
    public static Money operator -(Money left, Money right)
    {
        if (left is null || right is null)
        {
            throw new ArgumentNullException(left is null ? nameof(left) : nameof(right));
        }

        return From(left.Amount - right.Amount);
    }

    /// <summary>
    /// 等価性比較演算子
    /// </summary>
    public static bool operator ==(Money? left, Money? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    /// <summary>
    /// 非等価比較演算子
    /// </summary>
    public static bool operator !=(Money? left, Money? right) => !(left == right);

    /// <summary>
    /// より小さい比較
    /// </summary>
    public static bool operator <(Money? left, Money? right)
    {
        if (left is null || right is null)
            throw new ArgumentNullException(left is null ? nameof(left) : nameof(right));
        return left.Amount < right.Amount;
    }

    /// <summary>
    /// より大きい比較
    /// </summary>
    public static bool operator >(Money? left, Money? right)
    {
        if (left is null || right is null)
            throw new ArgumentNullException(left is null ? nameof(left) : nameof(right));
        return left.Amount > right.Amount;
    }

    /// <summary>
    /// 以下の比較
    /// </summary>
    public static bool operator <=(Money? left, Money? right)
    {
        if (left is null || right is null)
            throw new ArgumentNullException(left is null ? nameof(left) : nameof(right));
        return left.Amount <= right.Amount;
    }

    /// <summary>
    /// 以上の比較
    /// </summary>
    public static bool operator >=(Money? left, Money? right)
    {
        if (left is null || right is null)
            throw new ArgumentNullException(left is null ? nameof(left) : nameof(right));
        return left.Amount >= right.Amount;
    }
}
