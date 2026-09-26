using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Presentation.Shared.ViewModels;
using SupportAdvance.Presentation.WpfTrial.Views;

namespace SupportAdvance.Presentation.WpfTrial;

/// <summary>
/// WpfTrial プロジェクトの依存関係注入（DI）を設定するための拡張メソッドを提供するクラス
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// WpfTrial プロジェクトの依存関係をサービスコレクションに追加する拡張メソッド
    /// </summary>
    /// <param name="services">サービスコレクション</param>
    /// <returns>サービスコレクション</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> が <see langword="null"/> の場合</exception>
    public static IServiceCollection AddWpfTrialModules(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // ViewModels（Window に ViewModel を DI する）
        services.AddScoped<MainWindowViewModel>();
        services.AddScoped<LoginViewModel>();
        services.AddScoped<Form1ViewModel>();

        // BusinessDayClockの操作パネル（Presentation.Shared の共通 ViewModel）
        services.AddScoped<BusinessDayClockViewModel>();

        // Views（Window を DI コンテナから解決する）
        services.AddScoped<MainWindow>();
        services.AddScoped<LoginWindow>();

        return services;
    }
}
