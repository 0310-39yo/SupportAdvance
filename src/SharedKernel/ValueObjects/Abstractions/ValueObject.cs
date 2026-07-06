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
    ///
    /// 【初期値】false（Unset）
    ///
    /// 【設定方法】
    /// 派生クラスのコンストラクタで protected setter を使用して設定します。
    /// テストコード（protected スコープ内）でも設定可能です。
    /// </summary>
    public bool IsSet { get; protected set; }

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
    /// 等価性の比較に使用する値コンポーネントを取得する（IsSet を除く）
    /// 派生クラスは自身の値フィールドのみを列挙する
    /// IsSet は GetEqualityComponents で自動的に先頭に付加される
    /// </summary>
    /// <returns>等価性の比較に使用する値コンポーネントの列挙</returns>
    protected abstract IEnumerable<object?> GetValueComponents();

    /// <summary>
    /// 等価性の比較に使用するすべてのコンポーネントを取得する
    /// IsSet を先頭に追加し、GetValueComponents の結果を続ける
    /// ValueObjectComponentNormalizer で正規化される
    /// </summary>
    /// <returns>IsSet を含むすべての等価性コンポーネント</returns>
    protected IEnumerable<object?> GetEqualityComponents()
    {
        yield return IsSet;
        foreach (var component in GetValueComponents())
        {
            yield return component;
        }
    }

    /// <summary>
    /// このValueObjectのハッシュコードを取得する
    /// IsSet と値コンポーネントに基づいてハッシュコードを計算します
    /// </summary>
    /// <returns>ハッシュコード</returns>
    public override int GetHashCode()
    {
        var allComponents = GetEqualityComponents();
        var hash = 17;

        foreach (var component in allComponents)
        {
            unchecked
            {
                hash = hash * 31 + (component?.GetHashCode() ?? 0);
            }
        }

        return hash;
    }

    /// <summary>
    /// このValueObjectの文字列表現を取得する
    /// IsSetがfalse(Unset)の場合は"Unset"を返す
    /// IsSetがtrueの場合は、GetEqualityComponents()で取得したコンポーネント（IsSetを除く）の文字列表現をカンマ区切りで返す
    /// </summary>
    /// <returns>文字列表現</returns>
    public override string ToString()
    {
        if (!IsSet)
        {
            return "Unset";
        }

        // GetEqualityComponents は IsSet を先頭に yield し、その後に値コンポーネントを yield する
        // IsSet（最初の要素）をスキップして、値コンポーネントのみを取得
        var allComponents = GetEqualityComponents();
        var componentsStrings = allComponents
            .Skip(1)  // IsSet を先頭から除外
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
