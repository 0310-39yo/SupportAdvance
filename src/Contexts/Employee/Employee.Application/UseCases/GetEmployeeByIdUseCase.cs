namespace SupportAdvance.Contexts.Employee.Application.UseCases;
using SupportAdvance.Contexts.Employee.Application.Repositories;

using SupportAdvance.Contexts.Employee.Application.Dtos;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// ID で従業員を取得する Use Case
/// </summary>
public class GetEmployeeByIdUseCase
{
    private readonly IEmployeeRepository _repository;

    public GetEmployeeByIdUseCase(IEmployeeRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// 従業員を ID で検索
    /// </summary>
    public async Task<EmployeeDto?> ExecuteAsync(Guid employeeId)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Invalid EmployeeId", nameof(employeeId));

        var id = EmployeeId.From(employeeId);
        var employee = await _repository.GetByIdAsync(id);

        return employee?.ToDto();
    }
}
