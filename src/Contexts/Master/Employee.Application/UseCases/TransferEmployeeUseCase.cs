using SupportAdvance.Contexts.Master.Employee.Application.Repositories;
using SupportAdvance.Contexts.Master.Employee.Domain.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Master.Employee.Application.UseCases;

public class TransferEmployeeUseCase
{
    private readonly IEmployeeRepository _employeeRepository;

    public TransferEmployeeUseCase(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
    }

    public async Task ExecuteAsync(EmployeeId employeeId, RowId newDepartmentId)
    {
        ArgumentNullException.ThrowIfNull(employeeId);
        ArgumentNullException.ThrowIfNull(newDepartmentId);

        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
        {
            throw new InvalidOperationException($"Employee with ID '{employeeId.Value}' not found.");
        }

        employee.TransferDepartment(newDepartmentId);
        await _employeeRepository.UpdateAsync(employee);
    }
}
