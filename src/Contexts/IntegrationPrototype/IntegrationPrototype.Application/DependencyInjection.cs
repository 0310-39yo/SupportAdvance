using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Contexts.IntegrationPrototype.Application.UseCases;

namespace SupportAdvance.Contexts.IntegrationPrototype.Application;

/// <summary>
/// IntegrationPrototype Context の Application サービス登録
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// IntegrationPrototype Context の Use Cases を DI に登録
    /// </summary>
    public static IServiceCollection AddIntegrationPrototypeApplicationModels(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Cross-Context Use Cases（他の Context と連携）
        services.AddScoped<GetEmployeeByBizIdIntegrationUseCase>();

        return services;
    }
}
