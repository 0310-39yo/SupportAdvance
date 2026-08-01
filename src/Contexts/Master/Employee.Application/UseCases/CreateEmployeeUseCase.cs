using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Master.Employee.Application.Repositories;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using EmployeeEntity = SupportAdvance.Contexts.Master.Employee.Domain.Entities.Employee;

namespace SupportAdvance.Contexts.Master.Employee.Application.UseCases;

public class CreateEmployeeUseCase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IClock _clock;

    public CreateEmployeeUseCase(IEmployeeRepository employeeRepository, IClock clock)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<RowId> ExecuteAsync(
        string employeeNumber,
        string firstName,
        string lastName,
        string email,
        RowId departmentId,
        string jobTitle)
    {
        ArgumentNullException.ThrowIfNull(employeeNumber);
        ArgumentNullException.ThrowIfNull(firstName);
        ArgumentNullException.ThrowIfNull(lastName);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(departmentId);
        ArgumentNullException.ThrowIfNull(jobTitle);

        var existingByEmployeeNumber = await _employeeRepository.GetByEmployeeNumberAsync(employeeNumber);
        if (existingByEmployeeNumber != null)
        {
            throw new InvalidOperationException($"Employee with number '{employeeNumber}' already exists.");
        }

        var existingByEmail = await _employeeRepository.GetByEmailAsync(email);
        if (existingByEmail != null)
        {
            throw new InvalidOperationException($"Employee with email '{email}' already exists.");
        }

        var employee = new EmployeeEntity(
            employeeNumber,
            firstName,
            lastName,
            email,
            departmentId,
            jobTitle,
            _clock.JstNow);

        await _employeeRepository.CreateAsync(employee);

        return employee.Id;
    }
}
