using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Contexts.Authentication.Application.UseCases;

namespace SupportAdvance.Contexts.Authentication.Application;

/// <summary>
/// Authentication Context の Application層 DI 拡張メソッド
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Authentication Context の Application Models を DI コンテナに登録する
    /// </summary>
    /// <param name="services">登録先のサービスコレクション</param>
    /// <returns>メソッドチェーン用の <paramref name="services"/> 自身</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> が <see langword="null"/> の場合</exception>
    public static IServiceCollection AddAuthenticationApplicationModels(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Use Cases（Infrastructure/Application が DI 経由で注入される）
        services.AddScoped<AuthenticateLocalUserUseCase>();
        services.AddScoped<LogoutUseCase>();
        services.AddScoped<FindEmployeeByADUseCase>();

        return services;
    }
}
