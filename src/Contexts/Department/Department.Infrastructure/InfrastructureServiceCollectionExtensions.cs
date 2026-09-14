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
    /// Department Context の Infrastructure Models を DI コンテナに登録する
    /// </summary>
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
