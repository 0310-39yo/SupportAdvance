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

/// <summary>
/// 汎用 Infrastructure 層のサービスを DI コンテナーに登録する拡張メソッド群
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// 汎用 Infrastructure 層のサービスの登録と、ORM（Dapper／RepoDb）の初期化
    /// </summary>
    /// <param name="services">登録先のサービスコレクション</param>
    /// <param name="configuration">アプリケーションの構成。<c>AppSettings</c> セクション（<c>ClockSettings</c> を含む）が必須</param>
    /// <returns>メソッドチェーン用の <paramref name="services"/> 自身</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> または <paramref name="configuration"/> が <see langword="null"/> の場合</exception>
    /// <exception cref="InvalidOperationException">構成に <c>AppSettings</c> セクション、または <c>ClockSettings</c> がない場合</exception>
    /// <remarks>
    /// <para>【登録内容】<see cref="IClock"/>（Singleton）、<see cref="ISequenceProvider"/>（Singleton）、
    /// <see cref="IDbConnectionFactory"/>（Scoped）、<see cref="SqlQueryLoader"/>（Singleton）</para>
    /// <para>【副作用】Dapper の型ハンドラーと RepoDb の型マッパー・SQL Server 設定をプロセス全体に登録</para>
    /// <para>【事前条件】<see cref="IAppSettings"/> は HostBuilderFactory で登録済みであること</para>
    /// <para>【注意】各 Context の Infrastructure は対象外（Composition Root で別途登録）</para>
    /// </remarks>
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
