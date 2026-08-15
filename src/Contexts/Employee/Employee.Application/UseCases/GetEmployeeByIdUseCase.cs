namespace SupportAdvance.Contexts.Employee.Application.UseCases;

using Dtos;
using Repositories;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// ID で従業員を取得する Use Case
/// </summary>
public class GetEmployeeByIdUseCase(IEmployeeRepository repository)
{
    private readonly IEmployeeRepository _repository =
        repository ?? throw new ArgumentNullException(nameof(repository));

    /// <summary>
    /// 従業員を ID で検索
    /// </summary>
    public async Task<EmployeeDto?> ExecuteAsync(long employeeRowId)
    {
        if (employeeRowId <= 0)
        {
            throw new ArgumentException("Invalid EmployeeRowId", nameof(employeeRowId));
        }

        var id = EmployeeRowId.From(employeeRowId);
        var employee = await _repository.GetByIdAsync(id);

        return employee?.ToDto();
    }
}
