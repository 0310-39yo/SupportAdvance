using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Infrastructure.Mappers;

/// <summary>
/// DB の日時（<see cref="DateTime"/>）から <see cref="LocalDateTime"/> への変換の拡張メソッド
/// </summary>
/// <remarks>
/// <para>【責務】DB の型（<see cref="DateTime"/>）を Infrastructure の内側に閉じ込めるための変換。値オブジェクトは <c>TryFrom(LocalDateTime?)</c> のみを持ち、<see cref="DateTime"/> を知らない</para>
/// <para>【使い方】DB → Domain は、この変換の結果を値オブジェクトの <c>TryFrom</c> に渡す。Domain → DB は、既存の <see cref="LocalDateTime.Value"/>（<see cref="DateTime"/>）を使うため、専用の変換は不要</para>
/// <para>【時刻】DB の値は JST として解釈する。<see cref="DateTime.Kind"/> は <see cref="DateTimeKind.Unspecified"/> が前提</para>
/// <para>【参照】docs/Assistance/Guides/FromDbValue_ToDbValue_パターンガイド.md</para>
/// </remarks>
public static class DbDateTimeExtensions
{
    /// <summary>
    /// NOT NULL の日時列の値の <see cref="LocalDateTime"/> への変換
    /// </summary>
    /// <param name="dbValue">DB から読み込んだ日時（JST）</param>
    /// <returns>同じ日時を持つ <see cref="LocalDateTime"/></returns>
    /// <exception cref="ArgumentException"><paramref name="dbValue"/> の <see cref="DateTime.Kind"/> が <see cref="DateTimeKind.Unspecified"/> でない場合</exception>
    public static LocalDateTime ToLocalDateTime(this DateTime dbValue) => new(dbValue);

    /// <summary>
    /// NULL 許容の日時列の値の <see cref="LocalDateTime"/> への変換
    /// </summary>
    /// <param name="dbValue">DB から読み込んだ日時（JST）。DB の NULL は <see langword="null"/></param>
    /// <returns>同じ日時を持つ <see cref="LocalDateTime"/>。<paramref name="dbValue"/> が <see langword="null"/> の場合は <see langword="null"/></returns>
    /// <exception cref="ArgumentException"><paramref name="dbValue"/> の <see cref="DateTime.Kind"/> が <see cref="DateTimeKind.Unspecified"/> でない場合</exception>
    public static LocalDateTime? ToLocalDateTimeOrNull(this DateTime? dbValue) =>
        dbValue.HasValue ? new LocalDateTime(dbValue.Value) : null;
}
