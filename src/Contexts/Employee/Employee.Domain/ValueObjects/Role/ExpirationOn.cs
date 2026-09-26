using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Role;

/// <summary>
/// 有効終了日時（JST）を表す値オブジェクト
/// </summary>
/// <remarks>
/// <para>【責務】ロール・権限割り当ての有効終了日時の管理</para>
/// <para>【null契約】任意。無期限は <see cref="Unlimited"/>（<see cref="HasExpiration"/> が <see langword="false"/>）で表現</para>
/// <para>【時刻】JST の <see cref="LocalDateTime"/> で保持。DB の <c>DateTime</c> との変換は Infrastructure（Mapper）の担当。この型は <c>DateTime</c> の公開なし</para>
/// </remarks>
public sealed class ExpirationOn : ValueObject, IEquatable<ExpirationOn>
{
    /// <summary>
    /// 有効終了日時（JST）
    /// </summary>
    /// <value>無期限の場合は <see cref="LocalDateTime.MinValue"/></value>
    public LocalDateTime Value { get; }

    /// <summary>
    /// 有効期限が設定されているかどうかを示す値
    /// </summary>
    /// <value>設定済み（有効期限あり）の場合は <see langword="true"/></value>
    public new bool IsSet { get; }

    /// <summary>
    /// 有効期限があるかどうかを示す値
    /// </summary>
    /// <value>有効期限ありの場合は <see langword="true"/>、無期限の場合は <see langword="false"/></value>
    public bool HasExpiration => IsSet;

    /// <summary>
    /// 無期限の状態を表す <see cref="ExpirationOn"/>
    /// </summary>
    /// <value><see cref="HasExpiration"/> が <see langword="false"/> のインスタンス</value>
    public static ExpirationOn Unlimited => new(LocalDateTime.MinValue, false);

    /// <summary>
    /// 指定日時と設定状態による初期化。生成は <see cref="From"/>／<see cref="Unlimited"/> を使用
    /// </summary>
    /// <param name="value">有効終了日時（JST）。無期限の場合は <see cref="LocalDateTime.MinValue"/></param>
    /// <param name="isSet">設定済みかどうかを示す値</param>
    private ExpirationOn(LocalDateTime value, bool isSet)
    {
        Value = value;
        IsSet = isSet;
    }

    /// <summary>
    /// 指定された有効終了日を持つ <see cref="ExpirationOn"/> の生成
    /// </summary>
    /// <param name="value">有効終了日（JST）</param>
    /// <returns>有効期限ありのインスタンス</returns>
    public static ExpirationOn From(LocalDateTime value) => new(value, true);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ExpirationOn);

    /// <inheritdoc/>
    public bool Equals(ExpirationOn? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return IsSet == other.IsSet && Value == other.Value;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(IsSet, Value);

    /// <summary>
    /// 文字列表現の取得
    /// </summary>
    /// <returns>有効期限の文字列。未設定の場合は <c>無期限</c></returns>
    public override string ToString() => IsSet ? Value.ToString() : "無期限";

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return IsSet;
        yield return Value;
    }
}
