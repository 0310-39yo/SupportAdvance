using SupportAdvance.Contexts.Master.Employee.Domain.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using EmployeeEntity = SupportAdvance.Contexts.Master.Employee.Domain.Entities.Employee;

namespace SupportAdvance.Contexts.Master.Employee.Application.Repositories;

public interface IEmployeeRepository
{
    Task<EmployeeEntity?> GetByIdAsync(EmployeeId employeeId);
    Task<EmployeeEntity?> GetByEmployeeNumberAsync(string employeeNumber);
    Task<EmployeeEntity?> GetByEmailAsync(string email);
    Task<IEnumerable<EmployeeEntity>> GetByDepartmentIdAsync(RowId departmentId);
    Task CreateAsync(EmployeeEntity employee);
    Task UpdateAsync(EmployeeEntity employee);
    Task DeleteAsync(EmployeeId employeeId);
}

