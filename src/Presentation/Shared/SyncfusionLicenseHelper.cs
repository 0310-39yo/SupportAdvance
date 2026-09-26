using NLog;
using Syncfusion.Licensing;

namespace SupportAdvance.Presentation.Shared;

/// <summary>
/// Syncfusion ライセンスキーを環境変数から取得して登録するヘルパークラス
/// </summary>
public static class SyncfusionLicenseHelper
{
    /// <summary>
    /// ライセンスキーの環境変数名
    /// </summary>
    private const string LicenseKeyVariableName = "SyncfusionLicenseKey";

    /// <summary>
    /// ライセンスキーが未設定の場合に使用するデフォルト値
    /// </summary>
    private const string DefaultLicenseKey = "##SyncfusionLicense##";

    /// <summary>
    /// ロガー
    /// </summary>
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 環境変数から Syncfusion ライセンスキーを取得して登録
    /// 環境変数が設定されていない場合はデフォルト値の使用
    /// </summary>
    /// <remarks>
    /// ログ出力：
    /// - 登録開始時：Info
    /// - 環境変数からの取得時：Info / Warn（デフォルト使用時）
    /// - 登録完了時：Info
    /// - エラー時：Error
    /// </remarks>
    public static void RegisterLicenseFromEnvironment()
    {
        Logger.Info("Syncfusion ライセンス登録処理開始");

        try
        {
            var licenseKey = Environment.GetEnvironmentVariable(LicenseKeyVariableName);

            if (string.IsNullOrWhiteSpace(licenseKey))
            {
                Logger.Warn($"環境変数 '{LicenseKeyVariableName}' が設定されていません。デフォルト値を使用します。");
                licenseKey = DefaultLicenseKey;
            }
            else
            {
                Logger.Info($"環境変数 '{LicenseKeyVariableName}' からライセンスキーを取得しました。");
            }

            SyncfusionLicenseProvider.RegisterLicense(licenseKey);

            Logger.Info("Syncfusion ライセンス登録完了");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Syncfusion ライセンスキーの登録に失敗しました。");
            throw;
        }
    }
}
