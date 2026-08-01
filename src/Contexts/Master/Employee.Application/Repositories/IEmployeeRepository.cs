using SupportAdvance.Contexts.Master.Employee.Domain.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Master.Employee.Application.Repositories;

public interface IEmployeeRepository
{
    Task<Entities.Employee?> GetByIdAsync(RowId employeeId);
    Task<Entities.Employee?> GetByEmployeeNumberAsync(string employeeNumber);
    Task<Entities.Employee?> GetByEmailAsync(string email);
    Task<IEnumerable<Entities.Employee>> GetByDepartmentIdAsync(RowId departmentId);
    Task CreateAsync(Entities.Employee employee);
    Task UpdateAsync(Entities.Employee employee);
    Task DeleteAsync(RowId employeeId);
}
