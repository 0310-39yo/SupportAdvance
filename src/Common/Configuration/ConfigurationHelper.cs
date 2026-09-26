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
    /// <exception cref="ArgumentNullException"><paramref name="config"/> が <see langword="null"/> の場合</exception>
    /// <exception cref="ArgumentException"><paramref name="environmentName"/> が <see langword="null"/> または空文字の場合</exception>
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
