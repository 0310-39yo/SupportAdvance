namespace SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// データベース行を一意に識別する主キー値を表すValueObject
/// 0は未採番状態を表し、DB採番後に実際のrowIdに更新される
/// </summary>
public sealed class RowId : ValueObject, IEquatable<RowId>
{
    private readonly long _value;

    private RowId(long value)
    {
        _value = value;
        IsSet = true;
    }

    /// <summary>
    /// 指定された値からRowIdを作成する
    /// </summary>
    /// <param name="value">主キー値（0は未採番状態を表す）</param>
    /// <returns>RowIdのインスタンス</returns>
    public static RowId From(long value)
    {
        if (value < 0)
        {
            throw new ArgumentException("RowId must be non-negative.", nameof(value));
        }

        return new RowId(value);
    }

    /// <summary>
    /// 未採番状態のRowIdを作成する（value=0）
    /// DB採番後に実際のrowIdに更新される想定
    /// </summary>
    /// <returns>value=0のRowIdのインスタンス</returns>
    public static RowId New() => new(0);

    /// <summary>
    /// 保持する値を取得する
    /// </summary>
    public long Value => _value;

    /// <summary>
    /// 等価性判定のための値コンポーネントを返す
    /// </summary>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return _value;
    }

    /// <summary>
    /// 指定されたオブジェクトと等価かどうかを判定する
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as RowId);

    /// <summary>
    /// 指定されたRowIdと等価かどうかを判定する
    /// </summary>
    public bool Equals(RowId? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return _value == other._value;
    }

    /// <summary>
    /// ハッシュコードを取得する
    /// </summary>
    public override int GetHashCode() => _value.GetHashCode();

    /// <summary>
    /// 文字列表現を返す
    /// </summary>
    public override string ToString() => _value.ToString();
}
