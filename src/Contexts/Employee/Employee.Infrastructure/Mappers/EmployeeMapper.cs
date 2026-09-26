namespace SupportAdvance.Contexts.Employee.Infrastructure.Mappers;

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.Contexts.Employee.Infrastructure.Models;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// 従業員集約と DB モデル（<see cref="EmployeeDbModel"/> ほか）の相互変換を行うマッパー
/// </summary>
/// <remarks>
/// <para>【責務】値オブジェクト ↔ プリミティブ型の双方向変換。Domain は値オブジェクト、DB モデルはプリミティブ型で保持し、この型が橋渡しを行う</para>
/// <para>【注意】監査フィールド（UpdatedAt / UpdatedBy など）の設定は Repository の担当</para>
/// <para>【テスト容易性】状態と依存を持たない純粋な型変換。復元時に必要な時計は <see cref="ToDomainEntity"/> の引数で受け取る</para>
/// </remarks>
public class EmployeeMapper
{
    /// <summary>
    /// DB モデルからの従業員集約の復元（読み込み用）
    /// </summary>
    /// <param name="dbModel"><c>m_employees</c> の行</param>
    /// <param name="personDbModel"><c>m_persons</c> の行（1:1）</param>
    /// <param name="clock">復元する <see cref="Employee"/> に渡す時計（ドメインイベントの日時の取得元）</param>
    /// <param name="departmentMemberships"><c>m_department_memberships</c> の行（1:N）。<see langword="null"/> の場合は所属なし</param>
    /// <returns>復元した従業員集約</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dbModel"/>、<paramref name="personDbModel"/>、または <paramref name="clock"/> が <see langword="null"/> の場合</exception>
    /// <exception cref="ArgumentException">DB の値が値オブジェクトの検証に通らない場合（DB の整合性エラー。<see cref="ArgumentOutOfRangeException"/> を含む）</exception>
    public Employee ToDomainEntity(
        EmployeeDbModel dbModel,
        PersonDbModel personDbModel,
        IClock clock,
        List<DepartmentMembershipDbModel>? departmentMemberships = null)
    {
        ArgumentNullException.ThrowIfNull(dbModel);
        ArgumentNullException.ThrowIfNull(personDbModel);
        ArgumentNullException.ThrowIfNull(clock);

        // Person の変換（PersonMapper に委譲）
        var person = PersonMapper.ToDomainEntity(personDbModel);

        // ビジネス属性の変換
        var typeDivision = BizDivision.FromDbValue(dbModel.BizDivision);
        var bizId = BizId.From(dbModel.BizId);
        var bizCode = BizCode.From(typeDivision, bizId);

        var retiredOn = RetiredOn.Unset();
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
            clock,
            dbModel.RowVersion
        );
        return employee;
    }

    /// <summary>
    /// 従業員集約の DB モデルへの変換（Insert／Update 用）
    /// </summary>
    /// <param name="entity">変換する従業員</param>
    /// <returns><c>m_employees</c> の業務データのみ設定した DB モデル（監査列は未設定）</returns>
    public EmployeeDbModel ToDbModel(Employee entity) =>
        new()
        {
            RowId = entity.RowId.Value,
            BizDivision = entity.TypeDivision.ToDbValue(),
            BizId = entity.BizId.Value,
            RetiredOn = entity.RetiredOn.IsSet ? entity.RetiredOn.Value.Value : null
        };

    /// <summary>
    /// 人物の DB モデルへの変換
    /// </summary>
    /// <param name="person">変換する人物</param>
    /// <param name="employeeRowId">紐づく従業員の行ID（<c>employee_row_id</c>）</param>
    /// <returns><c>m_persons</c> の業務データのみ設定した DB モデル（監査列は未設定）</returns>
    /// <remarks>
    /// <para>【呼び出し元】Repository の SaveAsync</para>
    /// </remarks>
    public PersonDbModel ToPersonDbModel(Person person, long employeeRowId) =>
        PersonMapper.ToDbModel(person, employeeRowId);

    /// <summary>
    /// 部署メンバーシップの DB モデルへの変換
    /// </summary>
    /// <param name="membership">変換する部署メンバーシップ</param>
    /// <returns><c>m_department_memberships</c> の業務データのみ設定した DB モデル（監査列は未設定）</returns>
    /// <remarks>
    /// <para>【呼び出し元】Repository の AddAsync</para>
    /// </remarks>
    public DepartmentMembershipDbModel ToDepartmentMembershipDbModel(DepartmentMembership membership) =>
        new()
        {
            RowId = membership.RowId.Value,
            EmployeeRowId = membership.EmployeeRowId.Value,
            DepartmentRowId = membership.DepartmentRowId.Value,
            IsPrimary = membership.IsPrimary.Value,
            EndOn = membership.EndOn.HasEnded ? membership.EndOn.Value!.Value.Value : null,
            DepartmentName = membership.DepartmentName
        };
}
