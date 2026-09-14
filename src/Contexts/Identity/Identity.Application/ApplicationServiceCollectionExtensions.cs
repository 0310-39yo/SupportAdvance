using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Contexts.Identity.Application.Queries;
using SupportAdvance.Contexts.Identity.Application.UseCases;

namespace SupportAdvance.Contexts.Identity.Application;

/// <summary>
/// Identity Context の Application層 DI 拡張メソッド
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Identity Context の Application Models を DI コンテナに登録する
    /// </summary>
    public static IServiceCollection AddIdentityApplicationModels(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Use Cases（Infrastructure/Application が DI 経由で注入される）
        services.AddScoped<AuthenticateLocalUserUseCase>();
        services.AddScoped<LogoutUseCase>();
        services.AddScoped<FindEmployeeByADUseCase>();

        return services;
    }
}
