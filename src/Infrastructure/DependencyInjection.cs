using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Common.Configuration;

namespace SupportAdvance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureModels(this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // ステップ0: Dapper初期化（起動時の一度だけ）
        DapperTypeHandlerRegistration.Register();

        // ステップ1: appsettings.json から統合設定からバインド
        var appSettings = configuration
                              .GetSection("AppSettings")
                              .Get<AppSettings>()
                          ?? throw new InvalidOperationException("AppSettingのセクションが見つかりません。");

        // ステップ2: 統合設定を登録
        services.AddSingleton<IAppSettings>(appSettings);

        // ステップ3: 個別の責務別インターフェースを登録（後方互換性）
        services.AddSingleton<IApplicationSettings>(appSettings);
        services.AddSingleton<IFileSystemSettings>(appSettings);
        services.AddSingleton<IDatabaseSettings>(appSettings);

        // ステップ4: クロック設定を登録（IClockSettings は独立しているため、別途登録）
        services.AddSingleton(appSettings.ClockSettings);

        // ステップ5: IClock インターフェースを登録（IClockSettings から生成）
        var clockSettings = appSettings.ClockSettings
                            ?? throw new InvalidOperationException("ClockSettings が見つかりません。");

        var clockInstance = ClockFactory.CreateClock(clockSettings);
        services.AddSingleton<IClock>(clockInstance);  // ← インターフェース型で登録

        return services;
    }
}
