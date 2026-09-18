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
    /// <param name="services">登録先のサービスコレクション</param>
    /// <returns>メソッドチェーン用の <paramref name="services"/> 自身</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> が <see langword="null"/> の場合</exception>
    public static IServiceCollection AddIntegrationPrototypeApplicationModels(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Cross-Context Use Cases（他の Context と連携）
        services.AddScoped<GetEmployeeByBizIdIntegrationUseCase>();

        return services;
    }
}
