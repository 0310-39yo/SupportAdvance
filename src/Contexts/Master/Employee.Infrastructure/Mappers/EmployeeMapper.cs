using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Master.Employee.Domain.Entities;
using SupportAdvance.Contexts.Master.Employee.Infrastructure.DbModels;
using SupportAdvance.Infrastructure.Mappers;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using EmployeeEntity = SupportAdvance.Contexts.Master.Employee.Domain.Entities.Employee;

namespace SupportAdvance.Contexts.Master.Employee.Infrastructure.Mappers;

public class EmployeeMapper : IEntityMapper<EmployeeEntity, EmployeeDbModel, RowId>
{
    public EmployeeDbModel ToDbModel(EmployeeEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new EmployeeDbModel
        {
            RowId = entity.Id.Value,
            EmployeeNumber = entity.EmployeeNumber,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email,
            DepartmentId = entity.DepartmentId.Value,
            JobTitle = entity.JobTitle,
            IsActive = entity.IsActive,
            HireDate = entity.HireDate?.ToDateTime(),
        };
    }

    public EmployeeEntity ToDomainEntity(EmployeeDbModel dbModel, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbModel);
        ArgumentNullException.ThrowIfNull(clock);

        var hireDate = dbModel.HireDate.HasValue
            ? LocalDateTime.From(dbModel.HireDate.Value)
            : null;

        return new Employee(
            employeeNumber: dbModel.EmployeeNumber,
            firstName: dbModel.FirstName,
            lastName: dbModel.LastName,
            email: dbModel.Email,
            departmentId: RowId.From(dbModel.DepartmentId),
            jobTitle: dbModel.JobTitle,
            hireDate: hireDate,
            rowId: RowId.From(dbModel.RowId));
    }
}
