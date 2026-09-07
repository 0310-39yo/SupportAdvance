namespace SupportAdvance.Contexts.Employee.Infrastructure.Mappers;

using Models;
using Common.Clocks;
using Domain.Entities;
using Domain.ValueObjects.Department;
using Domain.ValueObjects.DepartmentMembership;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// Employee ドメインモデル ↔ EmployeeDbModel のマッピング
///
/// 【責務】ValueObject ↔ プリミティブ型の双方向変換
/// 【層の責務】
///   - Domain/Application: ValueObject（型安全性）
///   - DbModel: プリミティブ型（ORM マッピング）
///   - Mapper: 変換ロジック（層の橋渡し）
/// </summary>
public class EmployeeMapper(IClock clock)
{
    private readonly IClock _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    /// <summary>
    /// DbModel から Domain Entity に変換（読み込み用）
    /// 【責務】DB の プリミティブ型 → Domain の ValueObject に変換
    /// 【パラメータ】
    ///   - dbModel: m_employees テーブルのデータ
    ///   - personDbModel: m_persons テーブルのデータ（1:1 対応）
    ///   - departmentMemberships: m_department_memberships テーブルのデータ（1:N 対応）
    /// </summary>
    public Employee ToDomainEntity(
        EmployeeDbModel dbModel,
        PersonDbModel personDbModel,
        List<DepartmentMembershipDbModel>? departmentMemberships = null)
    {
        ArgumentNullException.ThrowIfNull(dbModel);
        ArgumentNullException.ThrowIfNull(personDbModel);

        // Person の変換（PersonMapper に委譲）
        var person = PersonMapper.ToDomainEntity(personDbModel);

        // ビジネス属性の変換
        var typeDivision = BizDivision.FromDbValue(dbModel.BizDivision);
        var bizId = BizId.From(dbModel.BizId);
        var bizCode = BizCode.From(typeDivision, bizId);

        var retiredOn = RetiredOn.Unset;
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

        var employee = Employee.Reconstruct(
            EmployeeRowId.From(dbModel.RowId),
            typeDivision,
            bizId,
            bizCode,
            retiredOn,
            person,
            memberships,
            dbModel.RowVersion
        );
        return employee;
    }

    /// <summary>
    /// Domain Entity から DbModel に変換（Insert/Update 用）
    /// 【責務】Domain の ValueObject → DB の プリミティブ型に変換
    /// </summary>
    public EmployeeDbModel ToDbModel(Employee entity) =>
        new()
        {
            RowId = entity.RowId.Value,
            BizDivision = entity.TypeDivision.ToDbValue(),
            BizId = entity.BizId.Value,
            RetiredOn = entity.RetiredOn.IsSet ? entity.RetiredOn.Value.Value : null,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = 0 // ← 実装計画では Repository で上書きされる
        };

    /// <summary>
    /// Domain Person を DbModel に変換する際のヘルパーメソッド
    /// 【責務】Employee.Person → PersonDbModel への変換
    /// 【呼び出し元】Repository の SaveAsync メソッド
    /// </summary>
    public PersonDbModel ToPersonDbModel(Person person, long employeeRowId) =>
        PersonMapper.ToDbModel(person, employeeRowId);
}
