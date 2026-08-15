namespace SupportAdvance.Contexts.Employee.Infrastructure.Mappers;

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Department;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.SharedKernel.ValueObjects;
using Models;

/// <summary>
/// Employee ドメインモデル ↔ EmployeeDbModel のマッピング
///
/// 【責務】ValueObject ↔ プリミティブ型の双方向変換
/// 【層の責務】
///   - Domain/Application: ValueObject（型安全性）
///   - DbModel: プリミティブ型（ORM マッピング）
///   - Mapper: 変換ロジック（層の橋渡し）
/// </summary>
public class EmployeeMapper
{
    private readonly IClock _clock;

    public EmployeeMapper(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>
    /// DbModel から Domain Entity に変換（読み込み用）
    /// 【責務】DB の プリミティブ型 → Domain の ValueObject に変換
    /// </summary>
    public Employee ToDomainEntity(EmployeeDbModel dbModel, Person person, List<DepartmentMembershipDbModel> departmentMemberships = null)
    {
        // DB値から ValueObject に変換（責務を ValueObject に委譲）
        var typeDivision = BizDivision.FromDbValue(dbModel.BizDivision);

        var bizId = BizId.From(dbModel.BizId);
        var bizCode = BizCode.From(typeDivision, bizId);

        RetiredOn retiredOn = RetiredOn.Unset;
        if (dbModel.RetiredOn.HasValue)
        {
            retiredOn = RetiredOn.From(new LocalDateTime(dbModel.RetiredOn.Value));
        }

        // DepartmentMembership を Entity に変換
        var memberships = new List<DepartmentMembership>();
        if (departmentMemberships != null)
        {
            foreach (var dm in departmentMemberships)
            {
                var isPrimary = dm.IsPrimary ? IsPrimary.Primary() : IsPrimary.Secondary();
                var endOn = dm.EndOn.HasValue ? EndOn.From(new LocalDateTime(dm.EndOn.Value)) : EndOn.Unset();

                var membership = DepartmentMembership.Create(
                    DepartmentMembershipRowId.From(dm.RowId),
                    EmployeeRowId.From(dm.EmployeeRowId),
                    DepartmentRowId.From(dm.DepartmentRowId),
                    isPrimary,
                    endOn,
                    dm.DepartmentName
                );
                memberships.Add(membership);
            }
        }

        return Employee.Reconstruct(
            EmployeeRowId.From(dbModel.RowId),
            typeDivision,
            bizId,
            bizCode,
            retiredOn,
            person,
            memberships
        );
    }

    /// <summary>
    /// Domain Entity から DbModel に変換（Insert/Update 用）
    /// 【責務】Domain の ValueObject → DB の プリミティブ型に変換
    /// 【注意】このメソッドは未実装（Insert/Update が実装される際に使用予定）
    /// </summary>
    public EmployeeDbModel ToDbModel(Employee entity)
    {
        return new EmployeeDbModel
        {
            RowId = entity.RowId.Value,
            BizDivision = entity.TypeDivision.ToDbValue(),
            BizId = entity.BizId.Value,
            RetiredOn = entity.RetiredOn.IsSet ? entity.RetiredOn.Value.Value : null
        };
    }
}
