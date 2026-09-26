namespace SupportAdvance.Presentation.Shared.Icons;

/// <summary>
/// <see cref="AppIcon"/> と Fluent UI System Icons の字形（アイコンフォントの文字）の対応表
/// </summary>
/// <remarks>
/// <para>【方式】アイコンフォント（<c>Assets/Fonts/FluentSystemIcons-Resizable.ttf</c>）を WpfTrial・WinTrial で共用し、アイコンを文字として描画する。
/// 拡大縮小・文字色の変更・高 DPI に強く、フォントファイルは 1 つのみ</para>
/// <para>【設計】UI（WPF／WinForms）に依存しない。描画は各アプリ側（WpfTrial の <c>FluentIcon</c> コントロール、WinTrial の <c>FluentIconFont</c>）が担当</para>
/// <para>【追加手順】</para>
/// <list type="number">
/// <item><description><see cref="AppIcon"/> に用途を表す名前を追加</description></item>
/// <item><description>Fluent UI System Icons のアイコン名（例: <c>ic_fluent_save_20_regular</c>）から、コードポイントを調べる
/// （<c>FluentSystemIcons-Resizable.json</c>。<see href="https://github.com/microsoft/fluentui-system-icons"/> の fonts フォルダー）</description></item>
/// <item><description>この対応表に「<see cref="AppIcon"/> の値 → コードポイント」を 1 行追加（対応表の検証テストが、追加漏れと重複を検出する）</description></item>
/// </list>
/// <para>【ライセンス】Fluent UI System Icons は MIT ライセンス（<c>Assets/Fonts/LICENSE.txt</c>）</para>
/// </remarks>
public static class FluentIconCatalog
{
    /// <summary>
    /// アイコンフォントのファミリー名
    /// </summary>
    public const string FontFamilyName = "FluentSystemIcons-Resizable";

    /// <summary>
    /// アイコンフォントのファイル名（各アプリのリソースとして同梱）
    /// </summary>
    public const string FontFileName = "FluentSystemIcons-Resizable.ttf";

    private static readonly IReadOnlyDictionary<AppIcon, int> CodePoints = new Dictionary<AppIcon, int>
    {
        [AppIcon.Customers] = 0xED75, // ic_fluent_people_20_regular
        [AppIcon.Orders] = 0xEF2D,    // ic_fluent_receipt_20_regular
        [AppIcon.List] = 0xF30C,      // ic_fluent_text_bullet_list_20_regular
        [AppIcon.Register] = 0xE6EB,  // ic_fluent_document_add_20_regular
        [AppIcon.Save] = 0xEF99       // ic_fluent_save_20_regular
    };

    /// <summary>
    /// 対応表に登録されているすべてのアイコンとコードポイント
    /// </summary>
    /// <value>アイコンの名前とコードポイントの組</value>
    public static IReadOnlyDictionary<AppIcon, int> All => CodePoints;

    /// <summary>
    /// アイコンを描画するための文字（フォントの字形）の取得
    /// </summary>
    /// <param name="icon">アイコンの名前</param>
    /// <returns>アイコンフォントの 1 文字（<see cref="FontFamilyName"/> のフォントで描画する）</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="icon"/> が対応表に無い場合</exception>
    public static string GetGlyph(AppIcon icon)
    {
        if (!CodePoints.TryGetValue(icon, out var codePoint))
        {
            throw new ArgumentOutOfRangeException(nameof(icon), icon, "アイコンが対応表に登録されていません");
        }

        return char.ConvertFromUtf32(codePoint);
    }
}
