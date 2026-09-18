namespace SupportAdvance.Contexts.Employee.Infrastructure.Mappers;

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.Contexts.Employee.Infrastructure.Models;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// Employee ドメインモデル ↔ EmployeeDbModel のマッピング
///
/// 【責務】ValueObject ↔ プリミティブ型の双方向変換
/// 【層の責務】
///   - Domain/Application: ValueObject（型安全性）
///   - DbModel: プリミティブ型（ORM マッピング）
///   - Mapper: 変換ロジック（層の橋渡し）
/// 【注意】監査フィールド（UpdatedAt/UpdatedBy）は Repository で管理
/// 【テスト容易性】Clock 依存なし（純粋な型変換）
/// </summary>
public class EmployeeMapper
{
    private readonly IClock _clock;

    /// <summary>
    /// <see cref="EmployeeMapper"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="clock">復元する <c>Employee</c> に渡す時計（ドメインイベントの日時取得用）</param>
    /// <exception cref="ArgumentNullException"><paramref name="clock"/> が <see langword="null"/> の場合</exception>
    public EmployeeMapper(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>
    /// DbModel から Domain Entity に変換（読み込み用）
    /// 【責務】DB の プリミティブ型 → Domain の ValueObject に変換
    /// 【パラメータ】
    ///   - dbModel: m_employees テーブルのデータ
    ///   - personDbModel: m_persons テーブルのデータ（1:1 対応）
    ///   - departmentMemberships: m_department_memberships テーブルのデータ（1:N 対応）
    /// </summary>
    /// <param name="dbModel"><c>m_employees</c> の行</param>
    /// <param name="personDbModel"><c>m_persons</c> の行（1:1）</param>
    /// <param name="departmentMemberships"><c>m_department_memberships</c> の行（1:N）。<see langword="null"/> の場合は所属なし</param>
    /// <returns>復元した従業員集約</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dbModel"/> または <paramref name="personDbModel"/> が <see langword="null"/> の場合</exception>
    /// <exception cref="ArgumentException">DB の値が値オブジェクトの検証に通らない場合（DB の整合性エラー。<see cref="ArgumentOutOfRangeException"/> を含む）</exception>
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
            _clock,
            dbModel.RowVersion
        );
        return employee;
    }

    /// <summary>
    /// Domain Entity から DbModel に変換（Insert/Update 用）
    /// 【責務】Domain の ValueObject → DB の プリミティブ型に変換
    /// 【注意】監査フィールド（UpdatedAt/UpdatedBy）は Repository で設定
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
    /// Domain Person を DbModel に変換する際のヘルパーメソッド
    /// 【責務】Employee.Person → PersonDbModel への変換
    /// 【呼び出し元】Repository の SaveAsync メソッド
    /// </summary>
    /// <param name="person">変換する人物</param>
    /// <param name="employeeRowId">紐づく従業員の行ID（<c>employee_row_id</c>）</param>
    /// <returns><c>m_persons</c> の業務データのみ設定した DB モデル（監査列は未設定）</returns>
    public PersonDbModel ToPersonDbModel(Person person, long employeeRowId) =>
        PersonMapper.ToDbModel(person, employeeRowId);

    /// <summary>
    /// Domain DepartmentMembership を DbModel に変換する際のヘルパーメソッド
    /// 【責務】DepartmentMembership → DepartmentMembershipDbModel への変換
    /// 【呼び出し元】Repository の AddAsync メソッド
    /// </summary>
    /// <param name="membership">変換する部署メンバーシップ</param>
    /// <returns><c>m_department_memberships</c> の業務データのみ設定した DB モデル（監査列は未設定）</returns>
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
