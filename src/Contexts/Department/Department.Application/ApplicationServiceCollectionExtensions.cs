namespace SupportAdvance.Contexts.Department.Application;

using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Application.Queries;
using SupportAdvance.Contexts.Department.Application.Queries;
using SupportAdvance.Contexts.Department.Domain.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Department = SupportAdvance.Contexts.Department.Domain.Entities.Department;

/// <summary>
/// Department Context の Application層 DI 拡張メソッド
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Department Context の Application Models を DI コンテナに登録する
    /// </summary>
    public static IServiceCollection AddDepartmentApplicationModels(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // ジェネリック Query Service パターン：他の BC が部署情報を照会するためのインターフェース
        services.AddScoped<IQueryService<Department, DepartmentRowId>, DepartmentQueryService>();

        return services;
    }
}
