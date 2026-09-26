using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Role;

/// <summary>
/// 有効開始日時（JST）を表す値オブジェクト
/// </summary>
/// <remarks>
/// <para>【責務】ロール・権限割り当ての有効開始日時の管理</para>
/// <para>【null契約】必須。常に値を持つ</para>
/// <para>【時刻】JST の <see cref="LocalDateTime"/> で保持。DB の <c>DateTime</c> との変換は Infrastructure（Mapper）の担当。この型は <c>DateTime</c> を公開しない</para>
/// </remarks>
public sealed class EffectiveAt : ValueObject, IEquatable<EffectiveAt>
{
    /// <summary>
    /// 有効開始日時（JST）
    /// </summary>
    public LocalDateTime Value { get; }

    /// <summary>
    /// 指定有効開始日時による初期化。生成は <see cref="From"/> を使用
    /// </summary>
    /// <param name="value">有効開始日時（JST）</param>
    private EffectiveAt(LocalDateTime value)
    {
        Value = value;
    }

    /// <summary>
    /// 指定された有効開始日時を持つ <see cref="EffectiveAt"/> の生成
    /// </summary>
    /// <param name="value">有効開始日時（JST）</param>
    /// <returns>指定日時を持つインスタンス</returns>
    public static EffectiveAt From(LocalDateTime value) => new(value);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as EffectiveAt);

    /// <inheritdoc/>
    public bool Equals(EffectiveAt? other)
    {
        if (other is null)
        {
            return false;
        }

        return Value == other.Value;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// 文字列表現の取得
    /// </summary>
    /// <returns>有効開始日時の文字列</returns>
    public override string ToString() => Value.ToString();

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return Value;
    }
}
