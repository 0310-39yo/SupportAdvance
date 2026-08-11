using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.DomainEvents;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Contexts.Employee.Domain.Entities;

/// <summary>
/// 従業員を表すドメインエンティティ（集約根）
///
/// 【集約ID】EmployeeRowId（long ベース）
/// 【責務】従業員の在職情報、部署配属、ロール、権限を管理
/// 【ライフサイクル】採用～退職までの全期間をトラッキング
/// </summary>
public sealed class Employee : AggregateRoot<EmployeeRowId>
{
    /// <summary>従業員種別区分（正社員/派遣/請負）</summary>
    public EmployeeTypeDivision TypeDivision { get; private set; }

    /// <summary>ビジネスID（従業員番号）</summary>
    public EmployeeBizId BizId { get; private set; }

    /// <summary>ビジネスコード（内部ID、表示用）</summary>
    public EmployeeBizCode BizCode { get; private set; }

    /// <summary>人事マスタ行ID（m_persons.row_id への外部参照）</summary>
    public PersonRowId PersonRowId { get; private set; }

    /// <summary>退職日（現職時は Unset）</summary>
    public RetiredOn? RetiredOn { get; private set; }

    /// <summary>
    /// 指定されたプロパティから Employee を生成する（プライベートコンストラクタ）
    /// </summary>
    private Employee(
        EmployeeRowId rowId,
        EmployeeTypeDivision typeDivision,
        EmployeeBizId bizId,
        EmployeeBizCode bizCode,
        PersonRowId personRowId,
        RetiredOn retiredOn)
    {
        RowId = rowId;
        TypeDivision = typeDivision;
        BizId = bizId;
        BizCode = bizCode;
        PersonRowId = personRowId;
        RetiredOn = retiredOn;
    }

    /// <summary>
    /// Employee を生成する（ファクトリメソッド）
    /// 【責務】Application/Infrastructure 層での Employee 生成・復元
    /// 【入力】RowId 事前採番済み
    /// </summary>
    public static Employee Create(
        EmployeeRowId rowId,
        EmployeeTypeDivision typeDivision,
        EmployeeBizId bizId,
        EmployeeBizCode bizCode,
        PersonRowId personRowId,
        RetiredOn? retiredOn = null) =>
        new(rowId, typeDivision, bizId, bizCode, personRowId, retiredOn ?? RetiredOn.Unset);

    /// <summary>
    /// 従業員が現在アクティブか判定する
    /// </summary>
    /// <param name="asOf">判定日時（JST）</param>
    /// <returns>現職の場合 true</returns>
    public bool IsActive(LocalDateTime asOf)
    {
        // 退職済みで判定日が退職日以降の場合は非アクティブ
        return RetiredOn?.HasRetired != true || !(asOf >= RetiredOn?.Value);
    }

    /// <summary>
    /// 従業員を退職させる
    /// 【責務】退職日の記録、ドメインイベント発行
    /// </summary>
    public void RetireEmployee(LocalDateTime retiredOn)
    {
        if (!IsActive(retiredOn))
        {
            throw new InvalidOperationException("Already retired.");
        }

        RetiredOn = RetiredOn.From(retiredOn);

        var retiredEvent = new EmployeeRetiredEvent(
            RowId.Value,
            TypeDivision.ToString(),
            BizId.ToString(),
            retiredOn);

        RaiseDomainEvent(retiredEvent);
    }

    /// <summary>
    /// Employee の文字列表現を取得する
    /// </summary>
    public override string ToString() => $"Employee(RowId={RowId.Value}, TypeDivision={TypeDivision}, BizId={BizId})";
}
