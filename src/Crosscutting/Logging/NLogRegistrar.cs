using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace SupportAdvance.Crosscutting.Logging;

/// <summary>
/// NLog登録支援ヘルパークラス
/// </summary>
public static class NLogRegistrar
{
    private static int _nlogConfigured; // 0 = 未実行、1 = 実行済み

    /// <summary>
    /// NLog登録処理
    /// </summary>
    /// <param name="context">ホストビルダーコンテキスト</param>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> が <see langword="null"/> の場合</exception>
    public static void RegisterNLog(HostBuilderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // 二重実行フラグを設定（設定ファイルは HostBuilderFactory で個別に読み込み済み）
        Interlocked.CompareExchange(ref _nlogConfigured, 1, 0);

        // デバッグ時に初期化状態フラグを出力して実行状態を確認できるようにする
        Debug.WriteLine($"NLogRegistrar: _nlogConfigured={_nlogConfigured}");
    }

    /// <summary>
    /// NLog構成ファイル追加処理
    /// </summary>
    /// <param name="builder">構成ビルダー</param>
    /// <param name="path">構成ファイルパス</param>
    /// <param name="optional">オプション指定</param>
    /// <param name="reloadOnChange">変更監視指定</param>
    /// <returns>構成ビルダー</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> が <see langword="null"/> の場合</exception>
    public static IConfigurationBuilder AddNLogConfiguration(this IConfigurationBuilder builder,
        string path = "Configuration/NLog.config", bool optional = true, bool reloadOnChange = true)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddXmlFile(path, optional, reloadOnChange);
    }
}
