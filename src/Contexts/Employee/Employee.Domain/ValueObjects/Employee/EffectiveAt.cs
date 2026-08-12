using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// 有効開始日時を表す ValueObject
/// 【型】LocalDateTime のラッパー
/// 【責務】ロール・権限割り当ての有効開始日時を管理
/// 【制約】常に値を持つ（必須）
/// </summary>
public sealed class EffectiveAt : ValueObject, IEquatable<EffectiveAt>
{
    /// <summary>有効開始日時（JST）</summary>
    public LocalDateTime Value { get; }

    /// <summary>
    /// 指定された有効開始日時から EffectiveAt を生成する（プライベートコンストラクタ）
    /// </summary>
    private EffectiveAt(LocalDateTime value)
    {
        Value = value;
    }

    /// <summary>
    /// 指定された有効開始日時から EffectiveAt を生成する
    /// </summary>
    /// <param name="value">有効開始日時（JST）</param>
    /// <returns>EffectiveAt インスタンス</returns>
    public static EffectiveAt From(LocalDateTime value) => new(value);

    /// <summary>
    /// DB値から EffectiveAt を復元する
    /// </summary>
    /// <param name="value">DB の datetime2 値</param>
    /// <param name="result">復元された EffectiveAt</param>
    /// <returns>復元成功時 true</returns>
    public static bool TryFromDbValue(DateTime value, out EffectiveAt result)
    {
        result = null!;

        try
        {
            result = From(new LocalDateTime(value));
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 指定された EffectiveAt と等価かどうかを判定する
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as EffectiveAt);

    /// <summary>
    /// 指定された EffectiveAt と等価かどうかを判定する
    /// </summary>
    public bool Equals(EffectiveAt? other)
    {
        if (other is null)
        {
            return false;
        }

        return Value == other.Value;
    }

    /// <summary>
    /// ハッシュコードを取得する
    /// </summary>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// 文字列表現を取得する
    /// </summary>
    public override string ToString() => Value.ToString();

    /// <summary>
    /// 等価性判定のための値コンポーネントを返す
    /// </summary>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return Value;
    }
}
