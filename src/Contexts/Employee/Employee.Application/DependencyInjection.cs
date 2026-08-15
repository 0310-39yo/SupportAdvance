using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Contexts.Employee.Application.UseCases;

namespace SupportAdvance.Contexts.Employee.Application;

/// <summary>
/// Employee Context の Application サービス登録
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Employee Context の Use Cases を DI に登録
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

        return services;
    }
}
