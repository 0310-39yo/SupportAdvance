using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Master.Employee.Application.Repositories;
using SupportAdvance.Contexts.Master.Employee.Domain.Entities;
using SupportAdvance.Contexts.Master.Employee.Infrastructure.DbModels;
using SupportAdvance.Contexts.Master.Employee.Infrastructure.Mappers;
using SupportAdvance.Infrastructure.Repositories;
using SupportAdvance.Infrastructure.Services;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using EmployeeEntity = SupportAdvance.Contexts.Master.Employee.Domain.Entities.Employee;

namespace SupportAdvance.Contexts.Master.Employee.Infrastructure.Repositories;

public class EmployeeRepository : RepositoryBase<EmployeeEntity, EmployeeDbModel, RowId>, IEmployeeRepository
{
    public EmployeeRepository(
        EmployeeMapper mapper,
        ICurrentUserService currentUser,
        IClock clock)
        : base(mapper, currentUser, clock)
    {
    }

    public Task<Employee?> GetByIdAsync(RowId employeeId)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task<Employee?> GetByEmployeeNumberAsync(string employeeNumber)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task<Employee?> GetByEmailAsync(string email)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task<IEnumerable<Employee>> GetByDepartmentIdAsync(RowId departmentId)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task CreateAsync(Employee employee)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task UpdateAsync(Employee employee)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task DeleteAsync(RowId employeeId)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }
}
