using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// 従業員の退職日（JST）を表す値オブジェクト
/// </summary>
/// <remarks>
/// <para>【用途】従業員の在職状況の判定</para>
/// <para>【null契約】任意。在職中は <see langword="null"/> ではなく <see cref="Unset"/>（<see cref="HasRetired"/> が <see langword="false"/>）で表現</para>
/// <para>【時刻】JST の <see cref="LocalDateTime"/> で保持。DB の <c>DateTime</c> との変換は Infrastructure（Mapper）の担当。この型は <c>DateTime</c> を公開しない</para>
/// </remarks>
public sealed class RetiredOn : ValueObject, IEquatable<RetiredOn>
{
    /// <summary>
    /// 未設定時のダミー値
    /// </summary>
    /// <remarks>
    /// <para>【注意】現在どこからも参照されていない値。未設定の判定には <see cref="HasRetired"/> を使用</para>
    /// </remarks>
    public const long UnsetValue = 0L;

    /// <summary>
    /// 退職日（JST）
    /// </summary>
    /// <value>未設定（在職中）の場合は <see cref="LocalDateTime.MinValue"/></value>
    public LocalDateTime Value { get; }

    /// <summary>
    /// 退職日が設定されているかどうかを示す値
    /// </summary>
    /// <value>設定済み（退職済み）の場合は <see langword="true"/></value>
    public new bool IsSet { get; }

    /// <summary>
    /// 退職済みかどうかを示す値
    /// </summary>
    /// <value>退職済みの場合は <see langword="true"/>。<see cref="IsSet"/> の、業務上の意味に合わせた別名</value>
    public bool HasRetired => IsSet;

    /// <summary>
    /// 在職中の状態を表す <see cref="RetiredOn"/> の生成
    /// </summary>
    /// <returns>在職中（<see cref="HasRetired"/> が <see langword="false"/>）のインスタンス（<see langword="null"/> なし）</returns>
    public static RetiredOn Unset() => new(LocalDateTime.MinValue, false);

    /// <summary>
    /// 指定日時と設定状態による初期化。生成は <see cref="From"/>／<see cref="Unset"/> を使用
    /// </summary>
    /// <param name="value">退職日（JST）。在職中の場合は <see cref="LocalDateTime.MinValue"/></param>
    /// <param name="isSet">設定済みかどうかを示す値</param>
    private RetiredOn(LocalDateTime value, bool isSet)
    {
        Value = value;
        IsSet = isSet;
    }

    /// <summary>
    /// 指定された退職日を持つ <see cref="RetiredOn"/> の生成
    /// </summary>
    /// <param name="value">退職日（JST）</param>
    /// <returns>退職済み（<see cref="HasRetired"/> が <see langword="true"/>）のインスタンス</returns>
    public static RetiredOn From(LocalDateTime value) => new(value, true);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as RetiredOn);

    /// <inheritdoc/>
    public bool Equals(RetiredOn? other)
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
    /// <returns>退職日の文字列。未設定（在職中）の場合は <c>現職</c></returns>
    public override string ToString() => IsSet ? Value.ToString() : "現職";

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return IsSet;
        yield return Value;
    }
}
