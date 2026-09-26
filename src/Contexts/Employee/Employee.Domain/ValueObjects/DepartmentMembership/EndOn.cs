using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;

/// <summary>
/// 異動終了日時（JST）を表す値オブジェクト
/// </summary>
/// <remarks>
/// <para>【責務】部署配属・ロール・権限の終了日の管理</para>
/// <para>【null契約】任意。無期限（継続中）は <see cref="Unset"/>（<see cref="Value"/> が <see langword="null"/>、<see cref="HasEnded"/> が <see langword="false"/>）で表現</para>
/// <para>【時刻】JST の <see cref="LocalDateTime"/> で保持。DB の <c>DateTime</c> との変換は Infrastructure（Mapper）の担当。この型は <c>DateTime</c> を公開しない</para>
/// </remarks>
public sealed class EndOn : ValueObject, IEquatable<EndOn>
{
    /// <summary>
    /// 終了日時（JST）
    /// </summary>
    /// <value>無期限の場合は <see langword="null"/></value>
    public LocalDateTime? Value { get; }

    /// <summary>
    /// 終了が設定されているかどうかを示す値
    /// </summary>
    /// <value>終了日が設定済みの場合は <see langword="true"/></value>
    public bool HasEnded => Value.HasValue;

    /// <summary>
    /// 無期限の状態を表す <see cref="EndOn"/> の生成
    /// </summary>
    /// <returns>終了日が未設定（無期限）のインスタンス</returns>
    public static EndOn Unset() => new(null);

    /// <summary>
    /// 指定終了日時による初期化。生成は <see cref="From"/>／<see cref="Unset"/> を使用
    /// </summary>
    /// <param name="value">終了日時（JST）。無期限の場合は <see langword="null"/></param>
    private EndOn(LocalDateTime? value)
    {
        Value = value;
    }

    /// <summary>
    /// 指定された終了日を持つ <see cref="EndOn"/> の生成
    /// </summary>
    /// <param name="value">終了日（JST）</param>
    /// <returns>終了日が設定済みのインスタンス</returns>
    public static EndOn From(LocalDateTime value) => new(value);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as EndOn);

    /// <inheritdoc/>
    public bool Equals(EndOn? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Value == other.Value;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => Value?.GetHashCode() ?? 0;

    /// <summary>
    /// 文字列表現の取得
    /// </summary>
    /// <returns>終了日の文字列。未設定の場合は <c>無期限</c></returns>
    public override string ToString() => Value?.ToString() ?? "無期限";

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return Value;
    }
}
