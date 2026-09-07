using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Application.Queries;
using SupportAdvance.Contexts.Employee.Application.Queries;
using SupportAdvance.Contexts.Employee.Application.UseCases;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using EmployeeEntity = SupportAdvance.Contexts.Employee.Domain.Entities.Employee;

namespace SupportAdvance.Contexts.Employee.Application;

/// <summary>
/// Employee Context の Application サービス登録
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Employee Context の Use Cases と Query Service を DI に登録
    /// </summary>
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
        services.AddScoped<IQueryService<EmployeeEntity, EmployeeRowId>, EmployeeQueryService>();

        return services;
    }
}
