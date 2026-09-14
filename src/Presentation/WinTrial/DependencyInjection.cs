using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Contexts.Employee.Application.UseCases;
using SupportAdvance.Presentation.WinTrial.ViewModels;
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
    public static IServiceCollection AddWinTrialModules(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Use Cases（Presentation が直接使用する場合）
        services.AddScoped<GetEmployeeByBizIdUseCase>();

        // ViewModels（ViewModel に Use Case を DI する）
        services.AddScoped<Form1ViewModel>();

        // Views（Form に ViewModel を DI する）
        services.AddScoped<Form1>();

        return services;
    }
}
