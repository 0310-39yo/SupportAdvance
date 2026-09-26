using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.SharedKernel.ValueObjects.Audit;

/// <summary>
/// エンティティの論理削除日時（JST）を表す値オブジェクト
/// </summary>
/// <remarks>
/// <para>【設計】<see cref="LocalDateTime"/> を内部値として保持し、Domain 層での型安全性を確保。未削除は <see cref="ValueObject.IsSet"/> が <see langword="false"/> の状態で表現</para>
/// <para>【null契約】未削除の状態は <see langword="null"/> ではなく <see cref="Unset"/>（<see cref="IsDeleted"/> が <see langword="false"/>）で表現。Domain 層での null 確認は不要。<see cref="TryFrom"/> は <see langword="null"/> の入力を <see cref="Unset"/> に変換して成功</para>
/// <para>【時刻】DB の <c>DateTime</c> との変換は Infrastructure（Mapper・Repository）の担当。この型は <c>DateTime</c> の公開なし</para>
/// <para>【参照】docs/Assistance/Guides/null厳格性設計ガイド.md</para>
/// </remarks>
/// <seealso cref="CreatedAt"/>
/// <seealso cref="UpdatedAt"/>
public sealed class DeletedAt : PrimitiveValueObject<LocalDateTime?>, IEquatable<DeletedAt>
{
    /// <summary>
    /// 指定日時と設定状態による初期化。生成は <see cref="From"/>／<see cref="Unset"/>／<see cref="TryFrom"/> を使用
    /// </summary>
    /// <param name="value">削除日時（JST）。未削除の場合は <see cref="LocalDateTime.MinValue"/></param>
    /// <param name="isSet">設定済みかどうかを示す値。既定は <see langword="true"/></param>
    /// <remarks>
    /// <para>【注意】検証（<see cref="Validate"/>）は基底クラスのコンストラクターで自動実行</para>
    /// </remarks>
    private DeletedAt(LocalDateTime? value, bool isSet = true) : base(value, isSet)
    {
    }

    /// <summary>
    /// 指定日時を持つ <see cref="DeletedAt"/> の生成
    /// </summary>
    /// <param name="value">削除日時（JST）。通常は <see cref="IClock.JstNow"/> から取得した値</param>
    /// <returns>削除済み（<see cref="IsDeleted"/> が <see langword="true"/>）のインスタンス</returns>
    /// <exception cref="ArgumentException"><paramref name="value"/> が <see cref="LocalDateTime.MinValue"/> または <see cref="LocalDateTime.MaxValue"/> の場合</exception>
    public static DeletedAt From(LocalDateTime value) => new(value, true);

    /// <summary>
    /// 未削除の状態を表す <see cref="DeletedAt"/> の生成
    /// </summary>
    /// <returns><see cref="IsDeleted"/> が <see langword="false"/> のインスタンス（<see langword="null"/> なし）</returns>
    /// <remarks>
    /// <para>【設計】<see cref="Value"/> は <see cref="LocalDateTime.MinValue"/> を保持。未削除かどうかの判定は <see cref="IsDeleted"/> を使用</para>
    /// </remarks>
    public static DeletedAt Unset() => new(LocalDateTime.MinValue, false);

    /// <summary>
    /// 日時（JST）からの <see cref="DeletedAt"/> 生成の試行。例外の送出なし
    /// </summary>
    /// <param name="input">削除日時（JST）。<see langword="null"/> は「未削除」（DB の値は Infrastructure が変換して渡す）</param>
    /// <param name="result">
    /// 成功した場合は生成したインスタンス（<paramref name="input"/> が <see langword="null"/> の場合は <see cref="Unset"/>）。
    /// 失敗した場合は <see langword="null"/>（使用禁止）
    /// </param>
    /// <returns>成功した場合、または <see langword="null"/> を <see cref="Unset"/> に変換した場合は <see langword="true"/>。値が検証に通らなかった場合は <see langword="false"/></returns>
    public static bool TryFrom(LocalDateTime? input, out DeletedAt result)
    {
        if (input == null || !input.HasValue)
        {
            result = Unset(); // ← null → Unset() で成功
            return true;
        }

        try
        {
            result = From(input.Value);
            return true;
        }
        catch (ArgumentException)
        {
            result = null!;
            return false;
        }
    }

    /// <summary>
    /// 削除日時（JST）
    /// </summary>
    /// <value>未削除の場合は <see cref="LocalDateTime.MinValue"/>。判定は <see cref="IsDeleted"/> を使用</value>
    public LocalDateTime? Value => ValueField;

    /// <summary>
    /// 削除済みかどうかを示す値
    /// </summary>
    /// <value>削除済みの場合は <see langword="true"/>。<see cref="ValueObject.IsSet"/> の、業務上の意味に合わせた別名</value>
    public bool IsDeleted => IsSet;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as DeletedAt);

    /// <inheritdoc/>
    public bool Equals(DeletedAt? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return ValueField == other.ValueField && IsSet == other.IsSet;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(ValueField, IsSet);

    /// <inheritdoc/>
    /// <remarks>
    /// <para>【注意】<see cref="ValueObject.IsSet"/> は基底クラスによる先頭への自動付加のため、ここでの含有は不要</para>
    /// </remarks>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return ValueField; // LocalDateTime? を返す
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException"><paramref name="normalized"/> が <see cref="LocalDateTime.MinValue"/> または <see cref="LocalDateTime.MaxValue"/> の場合（未削除を表す <see langword="null"/> は許容）</exception>
    public override void Validate(LocalDateTime? normalized)
    {
        base.Validate(normalized);

        // null は許容（未削除状態を表現）
        if (normalized == null || !normalized.HasValue)
        {
            return;
        }

        var value = normalized.Value;
        // LocalDateTime.MinValue や MaxValue は除外
        if (value == LocalDateTime.MinValue || value == LocalDateTime.MaxValue)
        {
            throw new ArgumentException(
                $"DeletedAt must be a valid system timestamp, not {nameof(LocalDateTime.MinValue)} or {nameof(LocalDateTime.MaxValue)}.");
        }
    }
}
