using Microsoft.Extensions.DependencyInjection;
using RepoDb;
using SupportAdvance.Contexts.Authentication.Application.Queries;
using SupportAdvance.Contexts.Authentication.Application.Repositories;
using SupportAdvance.Contexts.Authentication.Application.Services;
using SupportAdvance.Contexts.Authentication.Infrastructure.DbModels;
using SupportAdvance.Contexts.Authentication.Infrastructure.Mappers;
using SupportAdvance.Contexts.Authentication.Infrastructure.Queries;
using SupportAdvance.Contexts.Authentication.Infrastructure.Repositories;
using SupportAdvance.Contexts.Authentication.Infrastructure.Services;

namespace SupportAdvance.Contexts.Authentication.Infrastructure;

/// <summary>
/// Authentication Context の Infrastructure層 DI 拡張メソッド
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Authentication Context の Infrastructure Models の DI コンテナへの登録
    /// </summary>
    /// <param name="services">登録先のサービスコレクション</param>
    /// <returns>メソッドチェーン用の <paramref name="services"/> 自身</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> が <see langword="null"/> の場合</exception>
    /// <remarks>
    /// <para>【副作用】RepoDb の <c>UserAuthSessionDbModel</c> のテーブルマッピングをプロセス全体に登録（失敗時は無視）</para>
    /// </remarks>
    public static IServiceCollection AddAuthenticationInfrastructureModels(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // RepoDb テーブルマッピング登録（[Table] 属性が機能しない場合の明示的登録）
        try
        {
            FluentMapper.Entity<UserAuthSessionDbModel>()
                .Table("t_user_auth_sessions")
                .Primary(e => e.RowId)
                .Identity(e => e.RowId);
        }
        catch
        {
            // マッピング登録失敗時はスキップ（[Table] 属性でカバー）
        }

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
