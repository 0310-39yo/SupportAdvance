using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Crosscutting.Logging;

namespace SupportAdvance.Presentation.Shared;

/// <summary>
/// ホストビルダー生成支援ヘルパークラス
/// </summary>
public static class HostBuilderFactory
{
    /// <summary>
    /// ホストビルダー生成処理（詳細構成指定）
    /// </summary>
    /// <param name="environmentName">環境名</param>
    /// <param name="configureServices">サービス構成アクション</param>
    /// <param name="loggingExtra">追加ロギング構成アクション</param>
    /// <returns>IHostBuilderインスタンス</returns>
    public static IHostBuilder Create(string? environmentName = null,
        Action<HostBuilderContext, IServiceCollection>? configureServices = null,
        Action<ILoggingBuilder>? loggingExtra = null)
    {
        return Host.CreateDefaultBuilder().ConfigureAppConfiguration((context, config) =>
            {
                //appsettings.json などの構成読み込み（インフラ層の仕事）
                ConfigurationHelper.ConfigureApp(config, environmentName ?? EnvironmentInfo.Environment);

                var builtConfig = config.Build();
                var appsettings = builtConfig.GetSection(nameof(AppSettings)).Get<AppSettings>() ??
                                  new AppSettings();

                // NLog の GlobalDiagnosticsContext 設定（ログ基盤の初期化）
                GlobalDiagnosticsContext.Set("SolutionName", appsettings.SolutionName);
                GlobalDiagnosticsContext.Set("FolderName", appsettings.FolderName);
                GlobalDiagnosticsContext.Set("ProjectName", appsettings.ProjectName);
                GlobalDiagnosticsContext.Set("Environment", appsettings.ApplicationBuildType);

                // NLog 本体の初期化（ログファイルパス、フォーマット設定など）
                NLogInitializer.Initialize(appsettings, AppContext.BaseDirectory);
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                logging.AddNLog();
                loggingExtra?.Invoke(logging);
            })
            .ConfigureServices((context, services) =>
            {
                NLogRegistrar.RegisterNLog(context);
                services.Configure<AppSettings>(context.Configuration.GetSection(nameof(AppSettings)));

                // IAppSettings を DI に登録（すべての設定値を統一して取得するため）
                var appSettings = context.Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>()
                    ?? throw new InvalidOperationException("AppSettings section not found in configuration");
                services.AddSingleton<IAppSettings>(appSettings);

                // invoke layer registrations (Application/Infrastructure/Presentation etc.)
                configureServices?.Invoke(context, services);

                //// Register JsonSerializerOptions and allow infrastructure registrar to add converters
                //services.AddSingleton<JsonSerializerOptions>(sp =>
                //{
                //    var options = new JsonSerializerOptions();
                //    var registrar = sp.GetService<Advance.Domain.Serialization.IJsonConverterRegistrar>();
                //    registrar?.Register(options);
                //    return options;
                //});
            });
    }
}
