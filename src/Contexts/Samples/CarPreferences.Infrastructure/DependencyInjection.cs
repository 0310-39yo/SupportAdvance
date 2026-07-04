using Microsoft.Extensions.DependencyInjection;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCarPreferencesInfrastructureModels(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        // ここで、CarPreferences.Infrastructure に関連するサービスを登録します。
        // 例: services.AddScoped<ICarPreferenceRepository, CarPreferenceRepository>();
        return services;
    }
}
