using NLog;
using SupportAdvance.Common.Configuration;

namespace SupportAdvance.Crosscutting.Logging;

/// <summary>
/// NLog初期化支援ヘルパークラス
/// </summary>
public static class NLogInitializer
{
    /// <summary>
    /// NLog構成ファイル読み込み処理
    /// </summary>
    /// <param name="appsettings">アプリケーション設定</param>
    /// <param name="baseDirectory">ベースディレクトリ</param>
    public static void Initialize(AppSettings appsettings, string baseDirectory)
    {
        ArgumentNullException.ThrowIfNull(appsettings);
        ArgumentException.ThrowIfNullOrWhiteSpace(baseDirectory);

        // NLog のプロバイダーなどの構成情報は NLog.config をはじめから指定してロードする目的で、
        // このメソッドでは設定ファイルの読み込みのみを行う
        var candidates = new[]
        {
            Path.Combine(baseDirectory, "Configuration", "NLog.config"),
            Path.Combine(baseDirectory, "NLog.config")
        };

        var existing = candidates.FirstOrDefault(File.Exists);
        if (existing != null)
        {
            LogManager.Setup().LoadConfigurationFromFile(existing);
        }
        else
        {
            // 設定ファイルが見つからない場合、アプリケーションの初期化処理に失敗することはない
            // NLog のデフォルト設定により、どこにでもログ出力される
        }
    }
}
