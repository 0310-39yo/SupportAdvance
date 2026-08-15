using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Person;

namespace SupportAdvance.Contexts.Employee.Application.UseCases;
using SupportAdvance.Contexts.Employee.Application.Repositories;

using SupportAdvance.Contexts.Employee.Application.Dtos;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// 人事マスタ行ID で従業員群を取得する Use Case
/// </summary>
public class GetEmployeesByPersonRowIdUseCase
{
    private readonly IEmployeeRepository _repository;

    public GetEmployeesByPersonRowIdUseCase(IEmployeeRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// 人事マスタ行ID で従業員を検索
    /// </summary>
    public async Task<IReadOnlyList<EmployeeDto>> ExecuteAsync(long personRowId)
    {
        if (personRowId <= 0)
            throw new ArgumentException("PersonRowId must be > 0", nameof(personRowId));

        var id = PersonRowId.From(personRowId);
        var employees = await _repository.GetByPersonRowIdAsync(id);

        return employees.Select(e => e.ToDto()).ToList();
    }
}
