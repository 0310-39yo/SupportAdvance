using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Contexts.Department.Application.Repositories;
using SupportAdvance.Contexts.Department.Infrastructure.Mappers;
using SupportAdvance.Contexts.Department.Infrastructure.Repositories;

namespace SupportAdvance.Contexts.Department.Infrastructure;

/// <summary>
/// Department Context の Infrastructure層 DI 拡張メソッド
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Department Context の Infrastructure Models の DI コンテナへの登録
    /// </summary>
    /// <param name="services">登録先のサービスコレクション</param>
    /// <returns>メソッドチェーン用の <paramref name="services"/> 自身</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> が <see langword="null"/> の場合</exception>
    public static IServiceCollection AddDepartmentInfrastructureModels(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Mapper（純粋な型変換、Clock 依存なし）
        services.AddScoped<DepartmentMapper>();

        // Repository（DI される Mapper, IDbConnectionFactory, ICurrentUserService, IClock を使用）
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();

        return services;
    }
}
