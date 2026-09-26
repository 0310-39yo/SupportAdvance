using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.SharedKernel.ValueObjects.Audit;

/// <summary>
/// エンティティの作成日時（JST）を表す値オブジェクト
/// </summary>
/// <remarks>
/// <para>【設計】<see cref="LocalDateTime"/> を内部値として保持し、Domain 層での型安全性を確保</para>
/// <para>【null契約】必須。<see langword="null"/> の入力は <see cref="TryFrom"/> で失敗。Unset なし</para>
/// <para>【時刻】DB の <c>DateTime</c> との変換は Infrastructure（Mapper・Repository）の担当。この型は <c>DateTime</c> を公開しない</para>
/// <para>【参照】docs/Assistance/Guides/FromDbValue_ToDbValue_パターンガイド.md</para>
/// </remarks>
/// <seealso cref="UpdatedAt"/>
/// <seealso cref="DeletedAt"/>
public sealed class CreatedAt : PrimitiveValueObject<LocalDateTime>, IEquatable<CreatedAt>
{
    /// <summary>
    /// 指定日時による初期化。生成は <see cref="From"/>／<see cref="TryFrom"/> を使用
    /// </summary>
    /// <param name="value">作成日時（JST）</param>
    /// <remarks>
    /// <para>【注意】検証（<see cref="Validate"/>）は基底クラスのコンストラクターで自動実行</para>
    /// </remarks>
    private CreatedAt(LocalDateTime value) : base(value, true)
    {
    }

    /// <summary>
    /// 指定日時を持つ <see cref="CreatedAt"/> の生成
    /// </summary>
    /// <param name="value">作成日時（JST）。通常は <see cref="IClock.JstNow"/> から取得した値</param>
    /// <returns>指定日時を持つインスタンス</returns>
    /// <exception cref="ArgumentException"><paramref name="value"/> が <see cref="LocalDateTime.MinValue"/> または <see cref="LocalDateTime.MaxValue"/> の場合</exception>
    public static CreatedAt From(LocalDateTime value) => new(value);

    /// <summary>
    /// 日時（JST）からの <see cref="CreatedAt"/> 生成の試行。例外の送出なし
    /// </summary>
    /// <param name="input">作成日時（JST）。DB の値は Infrastructure が変換して渡す</param>
    /// <param name="result">成功した場合は生成したインスタンス。失敗した場合は <see langword="null"/>（使用禁止）</param>
    /// <returns>成功した場合は <see langword="true"/>。<paramref name="input"/> が <see langword="null"/> の場合、または値が検証に通らなかった場合は <see langword="false"/></returns>
    public static bool TryFrom(LocalDateTime? input, out CreatedAt result)
    {
        if (input == null || !input.HasValue)
        {
            result = null!;
            return false;
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
    /// 作成日時（JST）
    /// </summary>
    public LocalDateTime Value => ValueField;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as CreatedAt);

    /// <inheritdoc/>
    public bool Equals(CreatedAt? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return ValueField == other.ValueField;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => ValueField.GetHashCode();

    /// <inheritdoc/>
    /// <remarks>
    /// <para>【注意】<see cref="ValueObject.IsSet"/> は基底クラスが先頭に自動で付加するため、ここには含めない</para>
    /// </remarks>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return ValueField;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException"><paramref name="normalized"/> が <see cref="LocalDateTime.MinValue"/> または <see cref="LocalDateTime.MaxValue"/> の場合</exception>
    public override void Validate(LocalDateTime normalized)
    {
        base.Validate(normalized);

        // LocalDateTime.MinValue や MaxValue は除外
        if (normalized == LocalDateTime.MinValue || normalized == LocalDateTime.MaxValue)
        {
            throw new ArgumentException(
                $"CreatedAt must be a valid system timestamp, not {nameof(LocalDateTime.MinValue)} or {nameof(LocalDateTime.MaxValue)}.");
        }
    }
}
