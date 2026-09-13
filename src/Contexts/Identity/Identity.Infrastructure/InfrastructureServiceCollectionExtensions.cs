using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Contexts.Identity.Application.Queries;
using SupportAdvance.Contexts.Identity.Application.Services;
using SupportAdvance.Contexts.Identity.Domain.Repositories;
using SupportAdvance.Contexts.Identity.Infrastructure.Mappers;
using SupportAdvance.Contexts.Identity.Infrastructure.Queries;
using SupportAdvance.Contexts.Identity.Infrastructure.Repositories;
using SupportAdvance.Contexts.Identity.Infrastructure.Services;

namespace SupportAdvance.Contexts.Identity.Infrastructure;

/// <summary>
/// Identity Context の Infrastructure層 DI 拡張メソッド
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Identity Context の Infrastructure Models を DI コンテナに登録する
    /// </summary>
    public static IServiceCollection AddIdentityInfrastructureModels(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Mapper（純粋な型変換、Clock 依存なし）
        services.AddScoped<UserAuthSessionMapper>();

        // Repository（DI される Mapper, IDbConnectionFactory, ICurrentUserService, IClock を使用）
        services.AddScoped<IUserAuthSessionRepository, UserAuthSessionRepository>();

        // Query Service（ローカル認証情報マスターの検索）
        services.AddScoped<ILoginCredentialsQuery, LoginCredentialsQueryService>();

        // Password Hash Service（bcrypt ハッシング）
        services.AddScoped<IPasswordHashService, PasswordHashService>();

        return services;
    }
}
