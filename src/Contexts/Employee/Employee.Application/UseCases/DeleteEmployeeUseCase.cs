using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Employee.Application.UseCases;

using Repositories;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// 従業員を論理削除する Use Case
/// </summary>
public class DeleteEmployeeUseCase(IEmployeeRepository repository)
{
    private readonly IEmployeeRepository _repository =
        repository ?? throw new ArgumentNullException(nameof(repository));

    /// <summary>
    /// 従業員を論理削除
    /// </summary>
    /// <param name="employeeRowId">削除する従業員の行ID</param>
    /// <exception cref="ArgumentException"><paramref name="employeeRowId"/> が 0 以下の場合</exception>
    /// <exception cref="InvalidOperationException">従業員が見つからない場合</exception>
    public async Task ExecuteAsync(long employeeRowId)
    {
        if (employeeRowId <= 0)
        {
            throw new ArgumentException("Invalid EmployeeRowId", nameof(employeeRowId));
        }

        var id = EmployeeRowId.From(employeeRowId);

        // 存在確認
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null)
        {
            throw new InvalidOperationException($"Employee not found: {id}");
        }

        // 論理削除
        await _repository.DeleteAsync(id);
    }
}
