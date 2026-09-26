using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.SharedKernel.ValueObjects.Audit;

/// <summary>
/// エンティティの最終更新日時（JST）を表す値オブジェクト
/// </summary>
/// <remarks>
/// <para>【設計】<see cref="LocalDateTime"/> を内部値として保持し、Domain 層での型安全性を確保。未更新は <see cref="ValueObject.IsSet"/> が <see langword="false"/> の状態で表現</para>
/// <para>【null契約】未更新の状態は <see langword="null"/> ではなく <see cref="Unset"/>（<see cref="HasUpdated"/> が <see langword="false"/>）で表現。Domain 層での null 確認は不要。<see cref="TryFrom"/> は <see langword="null"/> の入力を <see cref="Unset"/> に変換して成功</para>
/// <para>【時刻】DB の <c>DateTime</c> との変換は Infrastructure（Mapper・Repository）の担当。この型は <c>DateTime</c> の公開なし</para>
/// <para>【参照】docs/Assistance/Guides/null厳格性設計ガイド.md</para>
/// </remarks>
/// <seealso cref="CreatedAt"/>
/// <seealso cref="DeletedAt"/>
public sealed class UpdatedAt : PrimitiveValueObject<LocalDateTime?>, IEquatable<UpdatedAt>
{
    /// <summary>
    /// 指定日時と設定状態による初期化。生成は <see cref="From"/>／<see cref="Unset"/>／<see cref="TryFrom"/> を使用
    /// </summary>
    /// <param name="value">更新日時（JST）。未更新の場合は <see cref="LocalDateTime.MinValue"/></param>
    /// <param name="isSet">設定済みかどうかを示す値。既定は <see langword="true"/></param>
    /// <remarks>
    /// <para>【注意】検証（<see cref="Validate"/>）は基底クラスのコンストラクターで自動実行</para>
    /// </remarks>
    private UpdatedAt(LocalDateTime? value, bool isSet = true) : base(value, isSet)
    {
    }

    /// <summary>
    /// 指定日時を持つ <see cref="UpdatedAt"/> の生成
    /// </summary>
    /// <param name="value">更新日時（JST）。通常は <see cref="IClock.JstNow"/> から取得した値</param>
    /// <returns>設定済み（<see cref="HasUpdated"/> が <see langword="true"/>）のインスタンス</returns>
    /// <exception cref="ArgumentException"><paramref name="value"/> が <see cref="LocalDateTime.MinValue"/> または <see cref="LocalDateTime.MaxValue"/> の場合</exception>
    public static UpdatedAt From(LocalDateTime value) => new(value, true);

    /// <summary>
    /// 未更新の状態を表す <see cref="UpdatedAt"/> の生成
    /// </summary>
    /// <returns><see cref="HasUpdated"/> が <see langword="false"/> のインスタンス（<see langword="null"/> なし）</returns>
    /// <remarks>
    /// <para>【設計】<see cref="Value"/> は <see cref="LocalDateTime.MinValue"/> を保持。未更新かどうかの判定は <see cref="HasUpdated"/> を使用</para>
    /// </remarks>
    public static UpdatedAt Unset() => new(LocalDateTime.MinValue, false);

    /// <summary>
    /// 日時（JST）からの <see cref="UpdatedAt"/> 生成の試行。例外の送出なし
    /// </summary>
    /// <param name="input">更新日時（JST）。<see langword="null"/> は「未更新」（DB の値は Infrastructure が変換して渡す）</param>
    /// <param name="result">
    /// 成功した場合は生成したインスタンス（<paramref name="input"/> が <see langword="null"/> の場合は <see cref="Unset"/>）。
    /// 失敗した場合は <see langword="null"/>（使用禁止）
    /// </param>
    /// <returns>成功した場合、または <see langword="null"/> を <see cref="Unset"/> に変換した場合は <see langword="true"/>。値が検証に通らなかった場合は <see langword="false"/></returns>
    public static bool TryFrom(LocalDateTime? input, out UpdatedAt result)
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
    /// 更新日時（JST）
    /// </summary>
    /// <value>未更新の場合は <see cref="LocalDateTime.MinValue"/>。判定は <see cref="HasUpdated"/> を使用</value>
    public LocalDateTime? Value => ValueField;

    /// <summary>
    /// 更新済みかどうかを示す値
    /// </summary>
    /// <value>更新済みの場合は <see langword="true"/>。<see cref="ValueObject.IsSet"/> の、業務上の意味に合わせた別名</value>
    public bool HasUpdated => IsSet;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as UpdatedAt);

    /// <inheritdoc/>
    public bool Equals(UpdatedAt? other)
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
    /// <exception cref="ArgumentException"><paramref name="normalized"/> が <see cref="LocalDateTime.MinValue"/> または <see cref="LocalDateTime.MaxValue"/> の場合（未更新を表す <see langword="null"/> は許容）</exception>
    public override void Validate(LocalDateTime? normalized)
    {
        base.Validate(normalized);

        // null は許容（未更新状態を表現）
        if (normalized == null || !normalized.HasValue)
        {
            return;
        }

        var value = normalized.Value;
        // LocalDateTime.MinValue や MaxValue は除外
        if (value == LocalDateTime.MinValue || value == LocalDateTime.MaxValue)
        {
            throw new ArgumentException(
                $"UpdatedAt must be a valid system timestamp, not {nameof(LocalDateTime.MinValue)} or {nameof(LocalDateTime.MaxValue)}.");
        }
    }
}
