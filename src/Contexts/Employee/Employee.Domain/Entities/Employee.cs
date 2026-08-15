using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.DomainEvents;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;
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
public sealed class Employee : AggregateRoot<EmployeeRowId> {
    /// <summary>
    /// 唯一の時計インスタンス（ドメインイベント発行時の日時取得に使用）
    /// </summary>
    private IClock Clock { get; set; }

    /// <summary>
    /// 従業員種別区分（正社員/派遣/請負）
    /// </summary>
    public BizDivision TypeDivision { get; private set; }

    /// <summary>
    /// ビジネスID（従業員番号）
    /// </summary>
    public BizId BizId { get; private set; }

    /// <summary>
    /// ビジネスコード（内部ID、表示用）
    /// </summary>
    public BizCode BizCode { get; private set; }

    /// <summary>
    /// 個人情報（内包子Entity）
    /// </summary>
    public Person Person { get; private set; }

    /// <summary>
    /// 退職日（現職時は Unset）
    /// </summary>
    public RetiredOn RetiredOn { get; private set; }

    /// <summary>
    /// 部署所属のコレクション（従業員が複数部署に所属可能）
    /// 【独立性】RetiredOn（雇用終了）と EndOn（配属終了）は独立している
    /// - 配置転換時：前部署の EndOn 更新、Employee.RetiredOn は変わらない
    /// - 退職時：Employee.RetiredOn 設定、配属終了日は別途管理
    /// 【責務】配属期間の管理（雇用期間はRetiredOnで管理）
    /// </summary>
    public IReadOnlyCollection<DepartmentMembership> DepartmentMemberships => _departmentMemberships.AsReadOnly();

    /// <summary>
    /// 部署所属の内部リスト
    /// </summary>
    private readonly List<DepartmentMembership> _departmentMemberships;

    /// <summary>
    /// 指定されたプロパティから Employee を生成する（プライベートコンストラクタ）
    /// </summary>
    private Employee(
        EmployeeRowId rowId,
        BizDivision typeDivision,
        BizId bizId,
        BizCode bizCode,
        RetiredOn retiredOn,
        Person person,
        List<DepartmentMembership> departmentMemberships
        )
    {
        RowId = rowId;
        TypeDivision = typeDivision;
        BizId = bizId;
        BizCode = bizCode;
        RetiredOn = retiredOn;
        Person = person;
        _departmentMemberships = departmentMemberships;
    }

    /// <summary>
    /// Employee を生成する（ファクトリメソッド）
    /// 【責務】Application/Infrastructure 層での Employee 生成・復元
    /// 【入力】RowId 事前採番済み、Person は新規生成される
    /// 【独立性】departmentMemberships は配属情報を指定（RetiredOn と独立して管理される）
    /// </summary>
    public static Employee Create(
        EmployeeRowId rowId,
        BizDivision typeDivision,
        BizId bizId,
        BizCode bizCode,
        RetiredOn? retiredOn,
        Person person,
        IEnumerable<DepartmentMembership> departmentMemberships) =>
        new(rowId, typeDivision, bizId, bizCode,retiredOn ?? RetiredOn.Unset, person, departmentMemberships.ToList());

    /// <summary>
    /// DB から読み込んだ値から Employee を復元する（ファクトリメソッド）
    /// 【独立性】departmentMemberships は配属情報（RetiredOn と独立して管理される）
    /// </summary>
    public static Employee Reconstruct(
        EmployeeRowId rowId,
        BizDivision typeDivision,
        BizId bizId,
        BizCode bizCode,
        RetiredOn retiredOn,
        Person person,
        IEnumerable<DepartmentMembership> departmentMemberships) =>
        new(rowId, typeDivision, bizId, bizCode, retiredOn, person,
            departmentMemberships.ToList());

    /// <summary>
    /// 従業員が現在アクティブか判定する
    /// </summary>
    /// <param name="asOf">判定日時（JST）</param>
    /// <returns>現職の場合 true</returns>
    public bool IsActive(LocalDateTime asOf)
    {
        // 退職済みで判定日が退職日以降の場合は非アクティブ
        if (!RetiredOn.HasRetired)
        {
            return true; // 退職していない = アクティブ
        }

        return asOf < RetiredOn.Value; // 退職していて、判定日が退職日より前 = アクティブ
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
    /// 部署メンバーシップを追加する
    /// 【責務】従業員の部署配置転換を記録
    /// 【呼び出し元】Application層の Use Case（例：TransferDepartmentUseCase）
    /// 【DB永続化】Repository.SaveAsync() で集約全体を保存時に DepartmentMemberships テーブルに反映
    /// </summary>
    public void AddDepartmentMembership(DepartmentMembership membership)
    {
        if (membership.EmployeeRowId != RowId)
        {
            throw new InvalidOperationException("DepartmentMembership must belong to this Employee.");
        }

        _departmentMemberships.Add(membership);
    }

    /// <summary>
    /// 部署メンバーシップを削除する
    /// 【責務】従業員の部署配置終了を記録
    /// 【呼び出し元】Application層の Use Case（例：TerminateDepartmentUseCase）
    /// 【DB永続化】Repository.SaveAsync() で集約全体を保存時に DepartmentMemberships テーブルから削除
    /// </summary>
    public void RemoveDepartmentMembership(DepartmentMembershipRowId membershipRowId)
    {
        _departmentMemberships.RemoveAll(m => m.RowId == membershipRowId);
    }

    /// <summary>
    /// Employee の文字列表現を取得する
    /// </summary>
    public override string ToString() => $"Employee(RowId={RowId.Value}, TypeDivision={TypeDivision}, BizId={BizId})";
}
