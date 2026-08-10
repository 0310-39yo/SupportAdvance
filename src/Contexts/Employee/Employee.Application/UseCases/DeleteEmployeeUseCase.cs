namespace SupportAdvance.Contexts.Employee.Application.UseCases;
using SupportAdvance.Contexts.Employee.Application.Repositories;

using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// 従業員を論理削除する Use Case
/// </summary>
public class DeleteEmployeeUseCase
{
    private readonly IEmployeeRepository _repository;

    public DeleteEmployeeUseCase(IEmployeeRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// 従業員を論理削除
    /// </summary>
    public async Task ExecuteAsync(Guid employeeId)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Invalid EmployeeId", nameof(employeeId));

        var id = EmployeeId.From(employeeId);

        // 存在確認
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null)
            throw new InvalidOperationException($"Employee not found: {id}");

        // 論理削除
        await _repository.DeleteAsync(id);
    }
}
