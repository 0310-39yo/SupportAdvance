using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Master.Employee.Domain.Entities;
using SupportAdvance.Contexts.Master.Employee.Infrastructure.DbModels;
using SupportAdvance.Infrastructure.Mappers;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Master.Employee.Infrastructure.Mappers;

public class DepartmentMapper : IEntityMapper<Department, DepartmentDbModel, RowId>
{
    public DepartmentDbModel ToDbModel(Department entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new DepartmentDbModel
        {
            RowId = entity.Id.Value,
            Name = entity.Name,
            Description = entity.Description,
            ParentDepartmentId = entity.ParentDepartmentId?.Value,
            IsActive = entity.IsActive,
        };
    }

    public Department ToDomainEntity(DepartmentDbModel dbModel, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbModel);
        ArgumentNullException.ThrowIfNull(clock);

        var parentDepartmentId = dbModel.ParentDepartmentId.HasValue
            ? RowId.From(dbModel.ParentDepartmentId.Value)
            : null;

        return new Department(
            name: dbModel.Name,
            description: dbModel.Description,
            parentDepartmentId: parentDepartmentId,
            rowId: RowId.From(dbModel.RowId));
    }
}
