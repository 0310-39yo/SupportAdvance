using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.DomainEvents;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Employee.Domain.Entities;

/// <summary>
/// 従業員を表すドメインエンティティ（集約根）
/// </summary>
/// <remarks>
/// <para>【集約ID】EmployeeRowId（long ベース）</para>
/// <para>【責務】従業員の在職情報、部署配属、ロール、権限を管理</para>
/// <para>【ライフサイクル】採用～退職までの全期間をトラッキング</para>
/// <para>【Application層インターフェース】IEmployee を実装（Context間での参照用）</para>
/// </remarks>
public sealed class Employee : AggregateRoot<EmployeeRowId>, IEmployee
{
    /// <summary>
    /// 唯一の時計インスタンス（ドメインイベント発行時の日時取得に使用）
    /// </summary>
    public IClock Clock { get; internal set; } = null!;

    /// <summary>
    /// 楽観ロックタイムスタンプ（concurrency control 用）
    /// </summary>
    /// <remarks>
    /// <para>【責務】DB更新時の競合検出</para>
    /// <para>【管理】更新時の Repository による新しい値での上書き</para>
    /// </remarks>
    public byte[] RowVersion { get; internal set; } = [];

    /// <summary>
    /// 従業員種別区分（正社員/派遣/請負）
    /// </summary>
    public BizDivision TypeDivision { get; private set; } = null!;

    /// <summary>
    /// ビジネスID（従業員番号）
    /// </summary>
    public BizId BizId { get; private set; } = null!;

    /// <summary>
    /// ビジネスコード（内部ID、表示用）
    /// </summary>
    public BizCode BizCode { get; private set; } = null!;

    /// <summary>
    /// 個人情報（内包子Entity）
    /// </summary>
    public Person Person { get; private set; } = null!;

    /// <summary>
    /// 退職日（現職時は Unset）
    /// </summary>
    public RetiredOn RetiredOn { get; private set; } = RetiredOn.Unset();

    /// <summary>
    /// 部署所属のコレクション（従業員が複数部署に所属可能）
    /// </summary>
    /// <remarks>
    /// <para>【独立性】RetiredOn（雇用終了）と EndOn（配属終了）は独立</para>
    /// <list type="bullet">
    /// <item><description>配置転換時：前部署の EndOn 更新、Employee.RetiredOn は不変</description></item>
    /// <item><description>退職時：Employee.RetiredOn 設定、配属終了日は別途管理</description></item>
    /// </list>
    /// <para>【責務】配属期間の管理（雇用期間はRetiredOnで管理）</para>
    /// </remarks>
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
        List<DepartmentMembership> departmentMemberships,
        IClock clock)
    {
        RowId = rowId;
        TypeDivision = typeDivision;
        BizId = bizId;
        BizCode = bizCode;
        RetiredOn = retiredOn;
        Person = person;
        _departmentMemberships = departmentMemberships;
        Clock = clock;
    }

    /// <summary>
    /// Employee を生成する（ファクトリメソッド）
    /// </summary>
    /// <param name="rowId">採番済みの従業員の行ID</param>
    /// <param name="typeDivision">従業員種別区分</param>
    /// <param name="bizId">従業員番号</param>
    /// <param name="bizCode">従業員コード（区分 + 番号）</param>
    /// <param name="retiredOn">退職日。<see langword="null"/> の場合は在職中（<see cref="RetiredOn.Unset"/>）</param>
    /// <param name="person">個人情報</param>
    /// <param name="departmentMemberships">部署への所属。0 件も可</param>
    /// <param name="clock">ドメインイベントの日時の取得元</param>
    /// <returns>生成した従業員（<c>RowVersion</c> は空）</returns>
    /// <remarks>
    /// <para>【責務】Application/Infrastructure 層での Employee 生成・復元</para>
    /// <para>【入力】RowId は事前採番済み、Person は新規生成</para>
    /// <para>【独立性】departmentMemberships は配属情報を指定（RetiredOn と独立して管理される）</para>
    /// </remarks>
    public static Employee Create(
        EmployeeRowId rowId,
        BizDivision typeDivision,
        BizId bizId,
        BizCode bizCode,
        RetiredOn? retiredOn,
        Person person,
        IEnumerable<DepartmentMembership> departmentMemberships,
        IClock clock) =>
        new(rowId, typeDivision, bizId, bizCode, retiredOn ?? RetiredOn.Unset(), person,
            departmentMemberships.ToList(), clock);

    /// <summary>
    /// DB から読み込んだ値から Employee を復元する（ファクトリメソッド）
    /// </summary>
    /// <param name="rowId">従業員の行ID</param>
    /// <param name="typeDivision">従業員種別区分</param>
    /// <param name="bizId">従業員番号</param>
    /// <param name="bizCode">従業員コード（区分 + 番号）</param>
    /// <param name="retiredOn">退職日（在職中の場合は Unset）</param>
    /// <param name="person">個人情報</param>
    /// <param name="departmentMemberships">部署への所属</param>
    /// <param name="clock">ドメインイベントの日時の取得元</param>
    /// <param name="rowVersion">楽観ロック用の値。<see langword="null"/> の場合は設定なし</param>
    /// <returns>復元した従業員</returns>
    /// <remarks>
    /// <para>【責務】DB の プリミティブ型 → Domain Entity に変換</para>
    /// <para>【パラメータ】rowVersion は楽観ロック用（更新時に競合検出）</para>
    /// <para>【独立性】departmentMemberships は配属情報（RetiredOn と独立して管理される）</para>
    /// </remarks>
    public static Employee Reconstruct(
        EmployeeRowId rowId,
        BizDivision typeDivision,
        BizId bizId,
        BizCode bizCode,
        RetiredOn retiredOn,
        Person person,
        IEnumerable<DepartmentMembership> departmentMemberships,
        IClock clock,
        byte[]? rowVersion = null)
    {
        var employee = new Employee(rowId, typeDivision, bizId, bizCode, retiredOn, person,
            departmentMemberships.ToList(), clock);
        if (rowVersion != null)
        {
            employee.RowVersion = rowVersion;
        }

        return employee;
    }

    /// <summary>
    /// 従業員が現在アクティブか判定
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
    /// </summary>
    /// <param name="retiredOn">退職日（JST）。この日時以降、<see cref="IsActive"/> の結果は <see langword="false"/></param>
    /// <exception cref="InvalidOperationException">設定済みの退職日が <paramref name="retiredOn"/> 以前の場合（その時点で退職済みの場合）</exception>
    /// <remarks>
    /// <para>【責務】退職日の記録、ドメインイベント発行</para>
    /// </remarks>
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
    /// 部署メンバーシップの追加
    /// </summary>
    /// <param name="membership">追加する部署メンバーシップ</param>
    /// <exception cref="InvalidOperationException"><paramref name="membership"/> がこの従業員に属していない場合</exception>
    /// <remarks>
    /// <para>【責務】従業員の部署配置転換を記録</para>
    /// <para>【呼び出し元】Application層の Use Case（例：TransferDepartmentUseCase）</para>
    /// <para>【DB永続化】Repository.SaveAsync() で集約全体を保存時に DepartmentMemberships テーブルに反映</para>
    /// </remarks>
    public void AddDepartmentMembership(DepartmentMembership membership)
    {
        if (membership.EmployeeRowId != RowId)
        {
            throw new InvalidOperationException("DepartmentMembership must belong to this Employee.");
        }

        _departmentMemberships.Add(membership);
    }

    /// <summary>
    /// 部署メンバーシップの削除
    /// </summary>
    /// <param name="membershipRowId">削除する部署メンバーシップの行ID。該当なしの場合は処理なし</param>
    /// <remarks>
    /// <para>【責務】従業員の部署配置終了を記録</para>
    /// <para>【呼び出し元】Application層の Use Case（例：TerminateDepartmentUseCase）</para>
    /// <para>【DB永続化】Repository.SaveAsync() で集約全体を保存時に DepartmentMemberships テーブルから削除</para>
    /// </remarks>
    public void RemoveDepartmentMembership(DepartmentMembershipRowId membershipRowId)
    {
        _departmentMemberships.RemoveAll(m => m.RowId == membershipRowId);
    }

    /// <summary>
    /// Employee の文字列表現の取得
    /// </summary>
    /// <returns><c>Employee(RowId=…, TypeDivision=…, BizId=…)</c> 形式のデバッグ用文字列。画面表示やデータの解析には使用禁止</returns>
    public override string ToString() => $"Employee(RowId={RowId.Value}, TypeDivision={TypeDivision}, BizId={BizId})";
}
