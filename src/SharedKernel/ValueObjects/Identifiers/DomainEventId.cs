namespace SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// ドメインイベントを一意に識別するValueObject
/// 各ドメインイベントは固有のGUID値を持つ
/// </summary>
public sealed class DomainEventId : ValueObject, IEquatable<DomainEventId>
{
    private readonly Guid _value;

    private DomainEventId(Guid value)
    {
        _value = value;
        IsSet = true;
    }

    /// <summary>
    /// 指定されたGUIDからDomainEventIdを作成する
    /// </summary>
    /// <param name="value">GUID値</param>
    /// <returns>DomainEventIdのインスタンス</returns>
    public static DomainEventId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("DomainEventId must not be empty GUID.", nameof(value));
        }

        return new DomainEventId(value);
    }

    /// <summary>
    /// 新しいDomainEventIdを生成する（新規GUID）
    /// </summary>
    /// <returns>新規GUIDを持つDomainEventIdのインスタンス</returns>
    public static DomainEventId New() => new(Guid.NewGuid());

    /// <summary>
    /// 保持するGUID値を取得する
    /// </summary>
    public Guid Value => _value;

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
    public override bool Equals(object? obj) => Equals(obj as DomainEventId);

    /// <summary>
    /// 指定されたDomainEventIdと等価かどうかを判定する
    /// </summary>
    public bool Equals(DomainEventId? other)
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
    /// 文字列表現を返す（GUID形式）
    /// </summary>
    public override string ToString() => _value.ToString();
}
