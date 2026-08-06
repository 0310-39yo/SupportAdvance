using SupportAdvance.Contexts.Master.Employee.Domain.Entities;
using SupportAdvance.Contexts.Master.Employee.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Master.Employee.Application.Repositories;

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(DepartmentId departmentId);
    Task<Department?> GetByNameAsync(string name);
    Task<IEnumerable<Department>> GetAllAsync();
    Task<IEnumerable<Department>> GetByParentIdAsync(DepartmentId parentDepartmentId);
    Task CreateAsync(Department department);
    Task UpdateAsync(Department department);
    Task DeleteAsync(DepartmentId departmentId);
}
