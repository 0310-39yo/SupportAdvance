using SupportAdvance.Contexts.Master.Employee.Domain.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using EmployeeEntity = SupportAdvance.Contexts.Master.Employee.Domain.Entities.Employee;

namespace SupportAdvance.Contexts.Master.Employee.Application.Repositories;

public interface IEmployeeRepository
{
    Task<EmployeeEntity?> GetByIdAsync(RowId employeeId);
    Task<EmployeeEntity?> GetByEmployeeNumberAsync(string employeeNumber);
    Task<EmployeeEntity?> GetByEmailAsync(string email);
    Task<IEnumerable<EmployeeEntity>> GetByDepartmentIdAsync(RowId departmentId);
    Task CreateAsync(EmployeeEntity employee);
    Task UpdateAsync(EmployeeEntity employee);
    Task DeleteAsync(RowId employeeId);
}
