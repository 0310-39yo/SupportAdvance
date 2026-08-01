using SupportAdvance.Contexts.Master.Employee.Domain.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Master.Employee.Application.Repositories;

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(RowId departmentId);
    Task<Department?> GetByNameAsync(string name);
    Task<IEnumerable<Department>> GetAllAsync();
    Task<IEnumerable<Department>> GetByParentIdAsync(RowId parentDepartmentId);
    Task CreateAsync(Department department);
    Task UpdateAsync(Department department);
    Task DeleteAsync(RowId departmentId);
}
