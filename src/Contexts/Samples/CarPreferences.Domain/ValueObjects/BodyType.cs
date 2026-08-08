namespace SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;

/// <summary>
/// 自動車ボディタイプを表す ValueObject
///
/// 【責務】ボディタイプの妥当性検証と等価性判定
/// 【不変性】一度作成されたら変更不可
/// 【検証】許可されたボディタイプのみ
/// </summary>
public sealed class BodyType : IEquatable<BodyType>
{
    /// <summary>
    /// ボディタイプの値（Sedan, SUV, Hatchback など）
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// 許可されたボディタイプ一覧
    /// </summary>
    public static readonly IReadOnlyList<string> AllowedTypes = new[]
    {
        "Sedan",
        "SUV",
        "Hatchback",
        "Coupe",
        "Wagon",
        "Van",
        "Truck",
        "Minivan",
        "Crossover",
        "Convertible"
    };

    private BodyType(string value)
    {
        Value = value;
    }

    /// <summary>
    /// ボディタイプを生成
    ///
    /// 【検証】
    /// - value が null または空文字列の場合は ArgumentException
    /// - value が AllowedTypes に含まれない場合は ArgumentException
    /// </summary>
    /// <param name="value">ボディタイプの値</param>
    /// <returns>BodyType インスタンス</returns>
    /// <exception cref="ArgumentException">値が無効な場合</exception>
    public static BodyType From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("BodyType value is required", nameof(value));
        }

        if (!AllowedTypes.Contains(value))
        {
            throw new ArgumentException(
                $"BodyType '{value}' is not allowed. " +
                $"Allowed types: {string.Join(", ", AllowedTypes)}",
                nameof(value));
        }

        return new BodyType(value);
    }

    /// <summary>
    /// ボディタイプの生成を試みる（nullable 版）
    ///
    /// 【用途】外部入力の安全な処理
    /// </summary>
    public static bool TryFrom(string? value, out BodyType result)
    {
        result = null!;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!AllowedTypes.Contains(value))
        {
            return false;
        }

        result = new BodyType(value);
        return true;
    }

    /// <summary>
    /// 等価性判定（値ベース）
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as BodyType);

    /// <summary>
    /// 等価性判定（型安全版）
    /// </summary>
    public bool Equals(BodyType? other) => other != null && Value == other.Value;

    /// <summary>
    /// ハッシュコード取得
    /// </summary>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// 文字列表現
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// 等価性比較演算子
    /// </summary>
    public static bool operator ==(BodyType? left, BodyType? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    /// <summary>
    /// 非等価比較演算子
    /// </summary>
    public static bool operator !=(BodyType? left, BodyType? right) => !(left == right);
}
