using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Contexts.Employee.Application.Repositories;
using SupportAdvance.Contexts.Employee.Infrastructure.Mappers;
using SupportAdvance.Contexts.Employee.Infrastructure.Repositories;

namespace SupportAdvance.Contexts.Employee.Infrastructure;

/// <summary>
/// Employee Context の Infrastructure サービス登録
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Employee Context の Repository と Mapper を DI に登録
    /// </summary>
    /// <param name="services">登録先のサービスコレクション</param>
    /// <returns>メソッドチェーン用の <paramref name="services"/> 自身</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> が <see langword="null"/> の場合</exception>
    public static IServiceCollection AddEmployeeInfrastructureModels(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Mapper を登録
        services.AddScoped<EmployeeMapper>();

        // Repository を登録（インターフェース型で登録し、実装を隠蔽）
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();

        return services;
    }
}
