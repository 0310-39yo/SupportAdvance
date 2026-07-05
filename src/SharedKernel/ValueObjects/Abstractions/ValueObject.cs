namespace SupportAdvance.SharedKernel.ValueObjects;

/// <summary>
/// ValueObjectの基底抽象クラス
/// すべてのValueObject派生クラスに対して、等価性の比較を提供する
/// 未設定状態(Unset)をnullに頼らず型で表現する仕組みを強制する
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// このValueObjectが設定されているかを示す値
    /// 既定値はfalse(Unset)
    /// 派生クラスのコンストラクタで明示的に設定する
    /// </summary>
    public bool IsSet { get; protected init; }

    /// <summary>
    /// 指定されたValueObjectと等価かどうかを判断する
    /// </summary>
    /// <param name="other">比較対象のValueObject</param>
    /// <returns>等価であればtrue、それ以外はfalse</returns>
    public bool Equals(ValueObject? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other.GetType() != GetType())
        {
            return false;
        }

        var normalizeComponents = ValueObjectComponentNormalizer.Normalize(this, GetEqualityComponents());
        var otherNormalizeComponents = ValueObjectComponentNormalizer.Normalize(other, other.GetEqualityComponents());

        return normalizeComponents.SequenceEqual(otherNormalizeComponents);
    }

    /// <summary>
    /// 指定されたValueObjectと等価かどうかを判断する
    /// </summary>
    /// <param name="obj">比較対象のValueObject</param>
    /// <returns>等価であればtrue、それ以外はfalse</returns>
    public override bool Equals(object? obj) => obj is ValueObject other && Equals(other);

    /// <summary>
    /// 等価性の比較に使用するコンポーネントを取得する
    /// ValueObjectComponentNormalizer経由で 先頭に自動的に付加される
    /// 派生クラスは自身の値フィールドのみを列挙する
    /// </summary>
    /// <returns>等価性の比較に使用するコンポーネントの列挙</returns>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <summary>
    /// このValueObjectのハッシュコードを取得する
    /// </summary>
    /// <returns>ハッシュコード</returns>
    public override int GetHashCode()
    {
        var normalizeComponents = ValueObjectComponentNormalizer.Normalize(this, GetEqualityComponents());

        return normalizeComponents
            .Aggregate(17, (current, component) =>
            {
                unchecked
                {
                    return current * 31 + (component?.GetHashCode() ?? 0);
                }
            });
    }

    /// <summary>
    /// このValueObjectの文字列表現を取得する
    /// IsSetがfalse(Unset)の場合は"Unset"を返す
    /// IsSetがtrueの場合は、GetEqualityComponents()で取得したコンポーネントの文字列表現をカンマ区切りで返す
    /// </summary>
    /// <returns>文字列表現</returns>
    public override string ToString()
    {
        if (!IsSet)
        {
            return "Unset";
        }

        var normalizeComponents = ValueObjectComponentNormalizer.Normalize(this, GetEqualityComponents());

        var componentsStrings = normalizeComponents
            .Skip(1)
            .Select(c => c?.ToString() ?? "null")
            .ToArray();

        return string.Join(", ", componentsStrings);
    }

    /// <summary>
    /// 2 つのValueObjectが等しいかどうかを判定します。
    /// </summary>
    /// <param name="left">左辺のオブジェクト</param>
    /// <param name="right">右辺のオブジェクト</param>
    /// <returns>等しい場合は true、そうでない場合は false</returns>
    public static bool operator ==(ValueObject? left, ValueObject? right)
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
    /// 2 つのValueObjectが等しくないかどうかを判定します。
    /// </summary>
    /// <param name="left">左辺のオブジェクト</param>
    /// <param name="right">右辺のオブジェクト</param>
    /// <returns>等しくない場合は true、等しい場合は false</returns>
    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}
