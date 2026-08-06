using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Master.Employee.Domain.ValueObjects;
using SupportAdvance.Contexts.Master.Employee.Infrastructure.DbModels;
using SupportAdvance.Contexts.Master.Employee.Infrastructure.Mappers;
using SupportAdvance.Contexts.Master.Employee.Application.Repositories;
using SupportAdvance.Infrastructure.Repositories;
using SupportAdvance.Infrastructure.Services;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using EmployeeEntity = SupportAdvance.Contexts.Master.Employee.Domain.Entities.Employee;

namespace SupportAdvance.Contexts.Master.Employee.Infrastructure.Repositories;

public class EmployeeRepository : RepositoryBase<EmployeeEntity, EmployeeDbModel, EmployeeId>, IEmployeeRepository
{
    public EmployeeRepository(
        EmployeeMapper mapper,
        ICurrentUserService currentUser,
        IClock clock)
        : base(mapper, currentUser, clock)
    {
    }

    public Task<EmployeeEntity?> GetByIdAsync(EmployeeId employeeId)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task<EmployeeEntity?> GetByEmployeeNumberAsync(string employeeNumber)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task<EmployeeEntity?> GetByEmailAsync(string email)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task<IEnumerable<EmployeeEntity>> GetByDepartmentIdAsync(RowId departmentId)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task CreateAsync(EmployeeEntity employee)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task UpdateAsync(EmployeeEntity employee)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }

    public Task DeleteAsync(EmployeeId employeeId)
    {
        throw new NotImplementedException("Database access layer to be implemented");
    }
}
