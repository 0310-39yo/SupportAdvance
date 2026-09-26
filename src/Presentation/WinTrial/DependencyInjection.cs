using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Contexts.Employee.Application.UseCases;
using SupportAdvance.Presentation.Shared;
using SupportAdvance.Presentation.Shared.ViewModels;
using SupportAdvance.Presentation.WinTrial.Views;

namespace SupportAdvance.Presentation.WinTrial;

/// <summary>
/// WinTrial プロジェクトの依存関係注入（DI）を設定するための拡張メソッドを提供するクラス
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// WinTrial プロジェクトの依存関係をサービスコレクションに追加する拡張メソッド
    /// </summary>
    /// <param name="services">サービスコレクション</param>
    /// <returns>サービスコレクション</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> が <see langword="null"/> の場合</exception>
    public static IServiceCollection AddWinTrialModules(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Use Cases（Presentation が直接使用する場合）
        services.AddScoped<GetEmployeeByBizIdUseCase>();

        // ViewModels（Presentation.Shared の共通 ViewModel。ViewModel に Use Case を DI する）
        // アプリケーションの識別情報（オープニング画面に表示する名前）
        services.AddSingleton(new AppIdentity("WinTrial"));

        services.AddScoped<MainWindowViewModel>();
        services.AddScoped<LoginViewModel>();
        services.AddScoped<Form1ViewModel>();

        // BusinessDayClockの操作パネル（Presentation.Shared の共通 ViewModel）
        services.AddScoped<BusinessDayClockViewModel>();

        // Views（Form に ViewModel を DI する）
        services.AddScoped<LoginDialog>();
        services.AddScoped<MainWindow>();

        return services;
    }
}
