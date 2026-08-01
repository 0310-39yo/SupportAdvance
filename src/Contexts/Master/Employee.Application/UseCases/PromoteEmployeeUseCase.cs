using SupportAdvance.Contexts.Master.Employee.Application.Repositories;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Master.Employee.Application.UseCases;

public class PromoteEmployeeUseCase
{
    private readonly IEmployeeRepository _employeeRepository;

    public PromoteEmployeeUseCase(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
    }

    public async Task ExecuteAsync(RowId employeeId, string newJobTitle)
    {
        ArgumentNullException.ThrowIfNull(employeeId);
        ArgumentNullException.ThrowIfNull(newJobTitle);

        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
        {
            throw new InvalidOperationException($"Employee with ID '{employeeId.Value}' not found.");
        }

        employee.ChangeJobTitle(newJobTitle);
        await _employeeRepository.UpdateAsync(employee);
    }
}
