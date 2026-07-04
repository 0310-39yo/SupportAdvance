using Microsoft.Extensions.Configuration;

namespace SupportAdvance.Common.Configuration;

/// <summary>
/// アプリケーション設定ファイル構成支援ヘルパークラス
/// </summary>
public static class ConfigurationHelper
{
    /// <summary>
    /// アプリケーション設定ファイル構成処理
    /// </summary>
    /// <param name="config">構成ビルダー</param>
    /// <param name="environmentName">環境名</param>
    public static void ConfigureApp(IConfigurationBuilder config, string? environmentName)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentException.ThrowIfNullOrEmpty(environmentName);

        config.SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(@"configs\appsettings.json", false, true)
            .AddJsonFile($@"configs\appsettings.{environmentName}.json", true, true)
            .AddJsonFile("appsettings.Intrinsic.json", true, true)
            .AddEnvironmentVariables();
    }
}
