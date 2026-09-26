using SupportAdvance.SharedKernel.ValueObjects;
using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Contexts.Department.Domain.ValueObjects;

/// <summary>
/// 部署の廃止日（JST）を表すオプションの値オブジェクト
/// </summary>
/// <remarks>
/// <para>【責務】<c>t_departments.abolished_on</c> に対応する値の管理と検証</para>
/// <para>【業務意味】論理削除ではなく、部署の廃止年月日の記録</para>
/// <para>【null契約】任意。廃止されていない部署は <see cref="Unset"/>（<see cref="IsAbolished"/> が <see langword="false"/>）で表現。<see cref="TryFrom"/> は <see langword="null"/> の入力を <see cref="Unset"/> に変換して成功</para>
/// <para>【時刻】JST の <see cref="LocalDateTime"/> で保持。DB の <c>DateTime</c> との変換は Infrastructure（Mapper）の担当。この型は <c>DateTime</c> の公開なし</para>
/// </remarks>
public sealed class AbolishedOn : ValueObject, IEquatable<AbolishedOn>
{
    /// <summary>
    /// 廃止日（JST）
    /// </summary>
    /// <value>未設定（廃止されていない）の場合は <see cref="LocalDateTime.MinValue"/>。判定は <see cref="IsAbolished"/> を使用</value>
    public LocalDateTime Value { get; }

    /// <summary>
    /// 廃止されているかどうかを示す値
    /// </summary>
    /// <value>廃止済みの場合は <see langword="true"/>。<see cref="IsSet"/> の、業務上の意味に合わせた別名</value>
    public bool IsAbolished => IsSet;

    /// <summary>
    /// 廃止日が設定されているかどうかを示す値
    /// </summary>
    public new bool IsSet { get; }

    /// <summary>
    /// 未設定状態による初期化。生成は <see cref="Unset"/> を使用
    /// </summary>
    /// <param name="isSet">設定済みかどうかを示す値</param>
    private AbolishedOn(bool isSet)
    {
        IsSet = isSet;
        Value = LocalDateTime.MinValue;
    }

    /// <summary>
    /// 指定廃止日による初期化。生成は <see cref="From"/> を使用
    /// </summary>
    /// <param name="value">廃止日（JST）</param>
    private AbolishedOn(LocalDateTime value)
    {
        IsSet = true;
        Value = value;
    }

    /// <summary>
    /// 廃止されていない部署を表す <see cref="AbolishedOn"/> の生成
    /// </summary>
    /// <returns><see cref="IsAbolished"/> が <see langword="false"/> のインスタンス</returns>
    public static AbolishedOn Unset() => new(false);

    /// <summary>
    /// 指定された廃止日を持つ <see cref="AbolishedOn"/> の生成
    /// </summary>
    /// <param name="value">廃止日（JST）</param>
    /// <returns>廃止済みのインスタンス</returns>
    public static AbolishedOn From(LocalDateTime value) => new(value);

    /// <summary>
    /// 廃止日からの <see cref="AbolishedOn"/> 生成の試行。例外の送出なし
    /// </summary>
    /// <param name="input">廃止日（JST）。<see langword="null"/> は「廃止されていない」（DB の値は Infrastructure が変換して渡す）</param>
    /// <param name="result">成功した場合は生成したインスタンス（<paramref name="input"/> が <see langword="null"/> の場合は <see cref="Unset"/>）。失敗した場合は <see langword="null"/>（使用禁止）</param>
    /// <returns>成功した場合、または <see langword="null"/> を <see cref="Unset"/> に変換した場合は <see langword="true"/>。検証に通らなかった場合は <see langword="false"/></returns>
    public static bool TryFrom(LocalDateTime? input, out AbolishedOn result)
    {
        result = null!;

        if (input is null)
        {
            result = Unset(); // null は Unset に変換（廃止なし）
            return true;
        }

        try
        {
            result = From(input.Value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as AbolishedOn);

    /// <inheritdoc/>
    public bool Equals(AbolishedOn? other)
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
    /// <returns>廃止日の文字列。未設定（廃止されていない）の場合は <c>Not Abolished</c></returns>
    public override string ToString()
    {
        return IsSet ? Value.ToString() : "Not Abolished";
    }

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return IsSet;
        if (IsSet)
        {
            yield return Value;
        }
    }
}
