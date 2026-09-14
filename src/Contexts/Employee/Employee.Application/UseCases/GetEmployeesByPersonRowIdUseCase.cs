namespace SupportAdvance.Contexts.Employee.Application.UseCases;

using Dtos;
using Repositories;
using Domain.ValueObjects.Person;

/// <summary>
/// 人事マスタ行ID で従業員群を取得する Use Case
/// </summary>
public class GetEmployeesByPersonRowIdUseCase(IEmployeeRepository repository)
{
    private readonly IEmployeeRepository _repository =
        repository ?? throw new ArgumentNullException(nameof(repository));

    /// <summary>
    /// 人事マスタ行ID で従業員を検索
    /// </summary>
    public async Task<IReadOnlyList<EmployeeDto>> ExecuteAsync(long personRowId)
    {
        if (personRowId <= 0)
        {
            throw new ArgumentException("PersonRowId must be > 0", nameof(personRowId));
        }

        var id = PersonRowId.From(personRowId);
        var employees = await _repository.GetByPersonRowIdAsync(id);

        return employees.Select(e => e.ToDto()).ToList();
    }
}
