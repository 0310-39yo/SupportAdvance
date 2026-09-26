using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Application.Queries;
using SupportAdvance.Contexts.Employee.Application.Queries;
using SupportAdvance.Contexts.Employee.Application.UseCases;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Employee.Application;

/// <summary>
/// Employee Context の Application サービス登録
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Employee Context の Use Cases と Query Service を DI に登録
    /// </summary>
    /// <param name="services">登録先のサービスコレクション</param>
    /// <returns>メソッドチェーン用の <paramref name="services"/> 自身</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> が <see langword="null"/> の場合</exception>
    /// <remarks>
    /// <para>【登録内容】<c>EmployeeQueryService</c> は 1 つのインスタンスを <c>IQueryServiceWithBizId</c>／<c>IQueryService</c>／<c>IEmployeeQueryService</c> の 3 つの型で共有（いずれも Scoped）</para>
    /// </remarks>
    public static IServiceCollection AddEmployeeApplicationModels(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Get Use Cases
        services.AddScoped<GetEmployeeByIdUseCase>();
        services.AddScoped<GetEmployeeByBizIdUseCase>();
        services.AddScoped<GetEmployeesByPersonRowIdUseCase>();

        // Create/Update/Delete Use Cases
        services.AddScoped<CreateEmployeeUseCase>();
        services.AddScoped<UpdateEmployeeUseCase>();
        services.AddScoped<DeleteEmployeeUseCase>();

        // Query Service（Context間でのドメインモデル参照）
        // IEmployee インターフェース経由で参照を提供（Domain Entity は隠蔽）
        // IQueryServiceWithBizId を実装（BizId での検索対応）
        // IEmployeeQueryService を実装（汎用層のインターフェース、BC間参照用）
        services.AddScoped<EmployeeQueryService>();
        services.AddScoped<IQueryServiceWithBizId<IEmployee, EmployeeRowId>>(sp =>
            sp.GetRequiredService<EmployeeQueryService>());
        services.AddScoped<IQueryService<IEmployee, EmployeeRowId>>(sp =>
            sp.GetRequiredService<EmployeeQueryService>());
        services.AddScoped<IEmployeeQueryService>(sp =>
            sp.GetRequiredService<EmployeeQueryService>());

        return services;
    }
}
