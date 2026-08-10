namespace SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// 集約を一意識別する GUID ベース ValueObject の基底クラス
///
/// 【責務】
/// - GUID 値の保持
/// - 等価性判定（GUID ベース）
///
/// 【継承】
/// 各集約は AggregateId を継承し、固有の ID クラスを定義
/// 例：OrderId, EmployeeId, UserPreferencesId など
///
/// 【型安全性】
/// - 異なる集約のID を型チェックで区別
/// - OrderId と EmployeeId は互換性なし
/// </summary>
public abstract class AggregateId : ValueObject
{
    /// <summary>
    /// GUID 値
    /// </summary>
    public Guid Value { get; protected set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="value">GUID 値</param>
    /// <exception cref="ArgumentException">value が Empty の場合</exception>
    protected AggregateId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AggregateId cannot be empty.", nameof(value));

        Value = value;
    }

    /// <summary>
    /// 等価性判定のための値コンポーネントを返す
    /// </summary>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// 文字列表現（デバッグ用）
    /// </summary>
    public override string ToString() => Value.ToString();
}
