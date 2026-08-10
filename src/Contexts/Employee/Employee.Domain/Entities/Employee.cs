namespace SupportAdvance.Contexts.Employee.Domain.Entities;

using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// 従業員を表すドメインエンティティ（集約根）
/// 【集約根ID】EmployeeId（GUID ベース）、Entity&lt;TId&gt;.Id で公開
/// 【公開プロパティ】EmployeeCode（M1234 形式）、PersonRowId（人事マスタ行ID）
/// 【責務】従業員ビジネスID の管理（属性は t_employee_attributes で管理）
/// </summary>
public sealed class Employee : Entity<EmployeeId>
{
    /// <summary>
    /// 従業員コード（M1234 形式）を取得する
    /// </summary>
    public EmployeeCode Code { get; private set; }

    /// <summary>
    /// 人事マスタ行ID（m_persons.row_id への外部参照）を取得する
    /// </summary>
    public PersonRowId PersonRowId { get; private set; }

    /// <summary>
    /// DB行ID（m_employees.row_id）を取得する
    /// </summary>
    public EmployeeRowId RowId { get; private set; }

    /// <summary>
    /// 指定されたプロパティからEmployeeを生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="employeeId">集約根ID（GUID ベース）</param>
    /// <param name="rowId">DB行ID</param>
    /// <param name="code">従業員コード（M1234 形式）</param>
    /// <param name="personRowId">人事マスタ行ID</param>
    private Employee(
        EmployeeId employeeId,
        EmployeeRowId rowId,
        EmployeeCode code,
        PersonRowId personRowId)
    {
        Id = employeeId;
        RowId = rowId;
        Code = code;
        PersonRowId = personRowId;
    }

    /// <summary>
    /// 新規 Employee を生成する（ファクトリメソッド）
    /// 【責務】Application 層での新規 Employee 生成
    /// </summary>
    /// <param name="employeeId">集約根ID（GUID ベース）</param>
    /// <param name="rowId">DB行ID（通常は 0L で初期化、Insert後に生成）</param>
    /// <param name="code">従業員コード（M1234 形式）</param>
    /// <param name="personRowId">人事マスタ行ID</param>
    /// <returns>生成された Employee インスタンス</returns>
    /// <remarks>
    /// パラメータはすべて検証済みの ValueObject として渡される。
    /// Employee レベルでの追加検証は不要。
    /// </remarks>
    public static Employee Create(
        EmployeeId employeeId,
        EmployeeRowId rowId,
        EmployeeCode code,
        PersonRowId personRowId)
    {
        return new(employeeId, rowId, code, personRowId);
    }

    /// <summary>
    /// DB から読み込んだ値から Employee を復元する（ファクトリメソッド）
    /// 【責務】Infrastructure 層での Employee 復元
    /// </summary>
    /// <param name="employeeId">集約根ID（GUID ベース）</param>
    /// <param name="rowId">DB行ID（生成済み）</param>
    /// <param name="code">従業員コード</param>
    /// <param name="personRowId">人事マスタ行ID</param>
    /// <returns>復元された Employee インスタンス</returns>
    /// <remarks>
    /// DB 値は既に検証済みと仮定。検証なしで復元。
    /// </remarks>
    public static Employee Reconstruct(
        EmployeeId employeeId,
        EmployeeRowId rowId,
        EmployeeCode code,
        PersonRowId personRowId)
    {
        return new(employeeId, rowId, code, personRowId);
    }

    /// <summary>
    /// Employee の文字列表現を取得する
    /// 【責務】ログ出力、デバッグ用の表現
    /// </summary>
    /// <returns>Employee の説明文字列（例："Employee(Id=..., Code=M1234)"）</returns>
    public override string ToString() => $"Employee(Id={Id.Value}, Code={Code})";
}
