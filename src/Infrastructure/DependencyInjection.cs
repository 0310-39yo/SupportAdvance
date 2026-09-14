using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RepoDb;
using SupportAdvance.Application.Abstractions.Identifiers;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Infrastructure.ORM.Dapper;
using SupportAdvance.Infrastructure.ORM.RepoDB;
using SupportAdvance.Infrastructure.Persistence;
using SupportAdvance.Infrastructure.Providers;

namespace SupportAdvance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureModels(this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // ステップ0a: Dapper初期化（起動時の一度だけ）
        DapperTypeHandlerRegistration.Register();

        // ステップ0b: RepoDb初期化（起動時の一度だけ）
        RepoDbTypeMapperRegistration.Register();

        // ステップ0c: RepoDb GlobalConfiguration 設定（SQL Server用）
        GlobalConfiguration
            .Setup()
            .UseSqlServer();

        // ステップ1: IAppSettings は HostBuilderFactory.cs で既に登録されている
        // services.Configure<AppSettings>() による登録済み
        // ここでは再登録不要 — 重複を避ける

        // ステップ2: AppSettings インスタンスを DI から取得（クロック設定用）
        var appSettings = configuration
                              .GetSection("AppSettings")
                              .Get<AppSettings>()
                          ?? throw new InvalidOperationException("AppSettingのセクションが見つかりません。");

        // ステップ3: IClock インターフェースを登録（IClockSettings から生成）
        var clockSettings = appSettings.ClockSettings
                            ?? throw new InvalidOperationException("ClockSettings が見つかりません。");

        var clockInstance = ClockFactory.CreateClock(clockSettings);
        services.AddSingleton<IClock>(clockInstance);

        // ステップ4: ISequenceProvider を登録（DB シーケンス采番用）
        services.AddSingleton<ISequenceProvider>(provider =>
        {
            var appSettings = provider.GetRequiredService<IAppSettings>();
            return new SequenceProvider(appSettings);
        });

        // ステップ5: IDbConnectionFactory を登録（Dapper 用 DB 接続ファクトリー）
        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

        // ステップ6: SqlQueryLoader を登録（SELECT 用 SQL ファイルローダー）
        services.AddSingleton<SqlQueryLoader>();

        // ステップ7: 各 Context の Infrastructure は Program.cs で直接登録
        // (循環参照を避けるため、汎用 Infrastructure は Context別層の参照を持たない)

        return services;
    }
}
