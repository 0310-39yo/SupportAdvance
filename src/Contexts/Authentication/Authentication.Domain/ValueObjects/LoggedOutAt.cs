using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Contexts.Authentication.Domain.ValueObjects;

/// <summary>
/// 認証セッションのログアウト日時（JST）を表す値オブジェクト
/// </summary>
/// <remarks>
/// <para>【用途】アプリケーション終了時のログアウト操作の記録。正常終了した場合のみ値を持ち、異常終了（クラッシュなど）の場合は未設定のまま（監査用）</para>
/// <para>【null契約】任意。未ログアウトは <see langword="null"/> ではなく <see cref="Unset"/>（<see cref="HasLoggedOut"/> が <see langword="false"/>）で表現。<see cref="TryFrom"/> は <see langword="null"/> の入力を <see cref="Unset"/> に変換して成功</para>
/// <para>【時刻】JST の <see cref="LocalDateTime"/> で保持。DB の <c>DateTime</c> との変換は Infrastructure（Mapper）の担当。この型は <c>DateTime</c> を公開しない</para>
/// <para>【参照】docs/Assistance/Guides/null厳格性設計ガイド.md</para>
/// </remarks>
public sealed class LoggedOutAt : ValueObject, IEquatable<LoggedOutAt>
{
    /// <summary>
    /// ログアウト日時（JST）
    /// </summary>
    /// <value>未ログアウトの場合は <see cref="LocalDateTime.MinValue"/>。判定は <see cref="HasLoggedOut"/> を使用</value>
    public LocalDateTime Value { get; }

    /// <summary>
    /// ログアウト日時が設定されているかどうかを示す値
    /// </summary>
    public new bool IsSet { get; }

    /// <summary>
    /// ログアウト済みかどうかを示す値
    /// </summary>
    /// <value>ログアウト済みの場合は <see langword="true"/>。<see cref="IsSet"/> の、業務上の意味に合わせた別名</value>
    public bool HasLoggedOut => IsSet;

    /// <summary>
    /// 指定日時と設定状態による初期化。生成は <see cref="From"/>／<see cref="Unset"/> を使用
    /// </summary>
    /// <param name="value">ログアウト日時（JST）。未ログアウトの場合は <see cref="LocalDateTime.MinValue"/></param>
    /// <param name="isSet">設定済みかどうかを示す値</param>
    private LoggedOutAt(LocalDateTime value, bool isSet)
    {
        Value = value;
        IsSet = isSet;
    }

    /// <summary>
    /// 未ログアウトの状態を表す <see cref="LoggedOutAt"/> の生成
    /// </summary>
    /// <returns><see cref="HasLoggedOut"/> が <see langword="false"/> のインスタンス（<see langword="null"/> なし）</returns>
    public static LoggedOutAt Unset() => new(LocalDateTime.MinValue, false);

    /// <summary>
    /// 指定日時を持つ <see cref="LoggedOutAt"/> の生成
    /// </summary>
    /// <param name="value">ログアウト日時（JST）。通常は <see cref="IClock.JstNow"/> から取得した値</param>
    /// <returns>ログアウト済み（<see cref="HasLoggedOut"/> が <see langword="true"/>）のインスタンス</returns>
    public static LoggedOutAt From(LocalDateTime value) => new(value, true);

    /// <summary>
    /// 日時（JST）からの <see cref="LoggedOutAt"/> 生成の試行。例外の送出なし
    /// </summary>
    /// <param name="input">ログアウト日時（JST）。<see langword="null"/> は「未ログアウト」（DB の値は Infrastructure が変換して渡す）</param>
    /// <param name="result">成功した場合は生成したインスタンス（<paramref name="input"/> が <see langword="null"/> の場合は <see cref="Unset"/>）。失敗した場合は <see langword="null"/>（使用禁止）</param>
    /// <returns>成功した場合、または <see langword="null"/> を <see cref="Unset"/> に変換した場合は <see langword="true"/>。値が検証に通らなかった場合は <see langword="false"/></returns>
    public static bool TryFrom(LocalDateTime? input, out LoggedOutAt result)
    {
        if (input is null)
        {
            result = Unset(); // null は Unset に変換（未ログアウト）
            return true;
        }

        if (input.Value == LocalDateTime.MinValue || input.Value == LocalDateTime.MaxValue)
        {
            result = null!;
            return false;
        }

        result = From(input.Value);
        return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as LoggedOutAt);

    /// <inheritdoc/>
    public bool Equals(LoggedOutAt? other)
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
    /// <returns>ログアウト日時の文字列。未設定の場合は <c>未ログアウト</c></returns>
    public override string ToString() => IsSet ? Value.ToString() : "未ログアウト";

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return IsSet;
        yield return Value;
    }
}
