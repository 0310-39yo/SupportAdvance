using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Master.Employee.Domain.ValueObjects;
using SupportAdvance.Contexts.Master.Employee.Infrastructure.DbModels;
using SupportAdvance.Infrastructure.Mappers;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using EmployeeEntity = SupportAdvance.Contexts.Master.Employee.Domain.Entities.Employee;

namespace SupportAdvance.Contexts.Master.Employee.Infrastructure.Mappers;

public class EmployeeMapper : IEntityMapper<EmployeeEntity, EmployeeDbModel, EmployeeId>
{
    public EmployeeDbModel ToDbModel(EmployeeEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new EmployeeDbModel
        {
            RowId = entity.RowId.Value,
            EmployeeId = entity.Id.Value,
            EmployeeNumber = entity.EmployeeNumber,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email,
            DepartmentId = entity.DepartmentId.Value,
            JobTitle = entity.JobTitle,
            IsActive = entity.IsActive,
            HireDate = entity.HireDate,
            CreatedAt = default,
            CreatedBy = 0,
            UpdatedAt = null,
            UpdatedBy = null,
            DeletedAt = null,
            DeletedBy = null,
        };
    }

    public EmployeeEntity ToDomainEntity(EmployeeDbModel dbModel, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbModel);
        ArgumentNullException.ThrowIfNull(clock);

        var employeeId = EmployeeId.From(dbModel.EmployeeId);
        var rowId = RowId.From(dbModel.RowId);

        return new EmployeeEntity(
            id: employeeId,
            employeeNumber: dbModel.EmployeeNumber,
            firstName: dbModel.FirstName,
            lastName: dbModel.LastName,
            email: dbModel.Email,
            departmentId: RowId.From(dbModel.DepartmentId),
            jobTitle: dbModel.JobTitle,
            hireDate: dbModel.HireDate,
            rowId: rowId,
            clock: clock);
    }
}
