using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Presentation.WinTrial.ViewModels;
using SupportAdvance.Presentation.WinTrial.Views;

namespace SupportAdvance.Presentation.WinTrial;

public static class DependencyInjection
{
    public static IServiceCollection AddWinTrialModules(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // ViewModel を登録（DI で注入可能にする）
        services.AddScoped<Form1ViewModel>();

        // メインフォームをスコープ登録（Program.cs の CreateScope と整合）
        services.AddScoped<Form1>();

        return services;
    }
}
