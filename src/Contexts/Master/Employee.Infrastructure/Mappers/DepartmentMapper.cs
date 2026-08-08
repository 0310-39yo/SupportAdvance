using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Master.Employee.Domain.Entities;
using SupportAdvance.Contexts.Master.Employee.Domain.ValueObjects;
using SupportAdvance.Contexts.Master.Employee.Infrastructure.DbModels;
using SupportAdvance.Infrastructure.Mappers;
using SupportAdvance.SharedKernel.ValueObjects.Audit;

namespace SupportAdvance.Contexts.Master.Employee.Infrastructure.Mappers;

public class DepartmentMapper : IEntityMapper<Department, DepartmentDbModel, DepartmentId>
{
    public DepartmentDbModel ToDbModel(Department entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new DepartmentDbModel
        {
            RowId = entity.RowId.Value,
            DepartmentId = entity.Id.Value,
            Name = entity.Name,
            Description = entity.Description,
            ParentDepartmentId = entity.ParentDepartmentId?.Value,
            IsActive = entity.IsActive,
            CreatedAt = default,
            CreatedBy = 0,
            UpdatedAt = null,
            UpdatedBy = null,
            DeletedAt = null,
            DeletedBy = null,
        };
    }

    public Department ToDomainEntity(DepartmentDbModel dbModel, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbModel);
        ArgumentNullException.ThrowIfNull(clock);

        var departmentId = DepartmentId.From(dbModel.DepartmentId);
        var rowId = RowId.From(dbModel.RowId);

        var parentDepartmentId = dbModel.ParentDepartmentId.HasValue
            ? RowId.From(dbModel.ParentDepartmentId.Value)
            : null;

        return new Department(
            id: departmentId,
            name: dbModel.Name,
            description: dbModel.Description,
            parentDepartmentId: parentDepartmentId,
            rowId: rowId,
            clock: clock);
    }
}
