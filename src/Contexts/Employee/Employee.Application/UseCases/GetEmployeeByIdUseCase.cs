using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

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
    /// <param name="employeeRowId">検索する従業員の行ID</param>
    /// <returns>見つかった従業員の DTO。見つからない場合は <see langword="null"/></returns>
    /// <exception cref="ArgumentException"><paramref name="employeeRowId"/> が 0 以下の場合</exception>
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
