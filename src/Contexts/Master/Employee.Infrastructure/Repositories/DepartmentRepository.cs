using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Master.Employee.Domain.Entities;
using SupportAdvance.Contexts.Master.Employee.Domain.ValueObjects;
using SupportAdvance.Contexts.Master.Employee.Infrastructure.DbModels;
using SupportAdvance.Contexts.Master.Employee.Infrastructure.Mappers;
using SupportAdvance.Contexts.Master.Employee.Application.Repositories;
using SupportAdvance.Infrastructure.Repositories;
using SupportAdvance.Infrastructure.Services;

namespace SupportAdvance.Contexts.Master.Employee.Infrastructure.Repositories;

public class DepartmentRepository : RepositoryBase<Department, DepartmentDbModel, DepartmentId>, IDepartmentRepository
{
    public DepartmentRepository(
        DepartmentMapper mapper,
        ICurrentUserService currentUser,
        IClock clock)
        : base(mapper, currentUser, clock)
    {
    }

    public Task<Department?> GetByIdAsync(DepartmentId departmentId)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task<Department?> GetByNameAsync(string name)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task<IEnumerable<Department>> GetAllAsync()
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task<IEnumerable<Department>> GetByParentIdAsync(DepartmentId parentDepartmentId)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task CreateAsync(Department department)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task UpdateAsync(Department department)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task DeleteAsync(DepartmentId departmentId)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }
}
