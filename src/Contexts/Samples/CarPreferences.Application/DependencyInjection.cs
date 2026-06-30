using Microsoft.Extensions.DependencyInjection;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCarPreferencesApplicationModels(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        // ここで、CarPreferences.Application に関連するサービスを登録します。
        // 例: services.AddScoped<ICarPreferenceService, CarPreferenceService>();
        return services;
    }
}
