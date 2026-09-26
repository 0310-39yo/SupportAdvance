using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Department.Domain.ValueObjects;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Department.Domain.Entities;

/// <summary>
/// 部署を表す集約ルート
/// </summary>
/// <remarks>
/// <para>【集約ID】<see cref="DepartmentRowId"/>（long ベース）</para>
/// <para>【責務】部署のマスターデータ（コード、名称、階層、親部署、管理者、廃止日）の管理</para>
/// <para>【不変条件】コードは一意（同じコードの部署は複数存在しない）。階層は整合（親部署の階層レベルより低い）。親部署のチェーンに循環参照なし</para>
/// <para>【ライフサイクル】作成から廃止までの全期間の追跡</para>
/// <para>【Context 間参照】他の BC への公開は <see cref="IDepartment"/> としてのみ</para>
/// </remarks>
public sealed class Department : AggregateRoot<DepartmentRowId>, IDepartment
{
    /// <summary>
    /// 楽観ロック用のタイムスタンプ
    /// </summary>
    /// <value>DB 更新時の競合検出用。更新時に Repository が新しい値で上書き</value>
    public byte[] RowVersion { get; internal set; } = [];

    /// <summary>
    /// 部署コード（ビジネスID。4 文字固定）
    /// </summary>
    public DepartmentCode DeptCode { get; private set; }

    /// <summary>
    /// 部署名
    /// </summary>
    public DepartmentName Name { get; private set; }

    /// <summary>
    /// 階層レベル（0:会社、1:本部、2:部、3:グループ、4:チーム）
    /// </summary>
    public HierarchyLevel Level { get; private set; }

    /// <summary>
    /// 親部署の行ID
    /// </summary>
    /// <value>トップレベルの場合は <see cref="ParentDepartmentRowId.Unset"/></value>
    public ParentDepartmentRowId ParentId { get; private set; }

    /// <summary>
    /// 部署管理者の従業員行ID
    /// </summary>
    /// <value>管理者未指定の場合は <see cref="ManagerEmployeeRowId.Unset"/></value>
    public ManagerEmployeeRowId ManagerId { get; private set; }

    /// <summary>
    /// 廃止日（JST）
    /// </summary>
    /// <value>廃止されていない場合は <see cref="AbolishedOn.Unset"/></value>
    public AbolishedOn AbolishedOn { get; private set; }

    private Department(
        DepartmentRowId rowId,
        DepartmentCode deptCode,
        DepartmentName name,
        HierarchyLevel level,
        ParentDepartmentRowId parentId,
        ManagerEmployeeRowId managerId,
        AbolishedOn abolishedOn)
    {
        RowId = rowId;
        DeptCode = deptCode;
        Name = name;
        Level = level;
        ParentId = parentId;
        ManagerId = managerId;
        AbolishedOn = abolishedOn;
    }

    /// <summary>
    /// 新規部署の生成
    /// </summary>
    /// <param name="rowId">採番済みの部署の行ID</param>
    /// <param name="deptCode">部署コード</param>
    /// <param name="name">部署名</param>
    /// <param name="level">階層レベル</param>
    /// <param name="parentId">親部署の行ID。<see langword="null"/> の場合は親なし（<see cref="ParentDepartmentRowId.Unset"/>）</param>
    /// <param name="managerId">管理者の従業員行ID。<see langword="null"/> の場合は未設定（<see cref="ManagerEmployeeRowId.Unset"/>）</param>
    /// <param name="abolishedOn">廃止日。<see langword="null"/> の場合は未廃止（<see cref="AbolishedOn.Unset"/>）</param>
    /// <returns>生成した部署（<c>RowVersion</c> は空。リポジトリは空の場合を新規作成として扱う）</returns>
    /// <exception cref="ArgumentNullException"><paramref name="deptCode"/> または <paramref name="name"/> が <see langword="null"/> の場合</exception>
    /// <remarks>
    /// <para>【呼び出し元】Application／Infrastructure 層での部署の生成。<paramref name="rowId"/> は事前採番済みであること</para>
    /// </remarks>
    public static Department Create(
        DepartmentRowId rowId,
        DepartmentCode deptCode,
        DepartmentName name,
        HierarchyLevel level,
        ParentDepartmentRowId? parentId = null,
        ManagerEmployeeRowId? managerId = null,
        AbolishedOn? abolishedOn = null)
    {
        ArgumentNullException.ThrowIfNull(deptCode);
        ArgumentNullException.ThrowIfNull(name);

        return new(
            rowId,
            deptCode,
            name,
            level,
            parentId ?? ParentDepartmentRowId.Unset(),
            managerId ?? ManagerEmployeeRowId.Unset(),
            abolishedOn ?? AbolishedOn.Unset());
    }

    /// <summary>
    /// DB から読み込んだ値による部署の復元
    /// </summary>
    /// <param name="rowId">部署の行ID</param>
    /// <param name="deptCode">部署コード</param>
    /// <param name="name">部署名</param>
    /// <param name="level">階層レベル</param>
    /// <param name="parentId">親部署の行ID（親なしの場合は Unset）</param>
    /// <param name="managerId">管理者の従業員行ID（未設定の場合は Unset）</param>
    /// <param name="abolishedOn">廃止日（未廃止の場合は Unset）</param>
    /// <param name="rowVersion">楽観ロック用の値。<see langword="null"/> の場合は設定なし</param>
    /// <returns>復元した部署</returns>
    /// <remarks>
    /// <para>【注意】引数の検証なし（DB の値を信頼）</para>
    /// <para>【用途】Infrastructure 層のマッパーからの呼び出し専用</para>
    /// </remarks>
    public static Department Reconstruct(
        DepartmentRowId rowId,
        DepartmentCode deptCode,
        DepartmentName name,
        HierarchyLevel level,
        ParentDepartmentRowId parentId,
        ManagerEmployeeRowId managerId,
        AbolishedOn abolishedOn,
        byte[]? rowVersion = null)
    {
        var department = new Department(rowId, deptCode, name, level, parentId, managerId, abolishedOn);
        if (rowVersion != null)
        {
            department.RowVersion = rowVersion;
        }

        return department;
    }

    /// <summary>
    /// 指定時点で部署が有効かどうかの判定（廃止されていないか）
    /// </summary>
    /// <param name="asOf">判定日時（JST）</param>
    /// <returns>廃止されていない場合、または <paramref name="asOf"/> が廃止日より前の場合は <see langword="true"/></returns>
    public bool IsActive(LocalDateTime asOf)
    {
        // 廃止されていない場合は常に有効
        if (!AbolishedOn.IsAbolished)
        {
            return true;
        }

        // 廃止されていて、判定日が廃止日より前なら有効、以後なら無効
        return asOf < AbolishedOn.Value;
    }

    /// <summary>
    /// 親部署の更新（不変条件の確認つき）
    /// </summary>
    /// <param name="newParentId">新しい親部署の行ID。Unset の場合はトップレベル化</param>
    /// <exception cref="InvalidOperationException"><paramref name="newParentId"/> が自分自身の場合</exception>
    /// <remarks>
    /// <para>【責務】親部署の妥当性の検証（自分自身の禁止）</para>
    /// <para>【注意】循環参照の完全な確認と親部署の存在確認は、他の部署の情報が必要なため Repository の担当</para>
    /// </remarks>
    public void UpdateParent(ParentDepartmentRowId newParentId)
    {
        // Unset（トップレベル化）は常に許可
        if (!newParentId.HasParent)
        {
            ParentId = newParentId;
            return;
        }

        // 自分自身を親とすることは禁止
        if (newParentId.Value == RowId.Value)
        {
            throw new InvalidOperationException("部署を自身の親部署として設定することはできません。");
        }

        // 循環参照の完全チェックは Repository が実施（他部署の情報が必要）
        ParentId = newParentId;
    }

    /// <summary>
    /// 管理者の更新
    /// </summary>
    /// <param name="newManagerId">新しい管理者の従業員行ID。Unset の場合は管理者なし</param>
    public void UpdateManager(ManagerEmployeeRowId newManagerId)
    {
        ManagerId = newManagerId;
    }

    /// <summary>
    /// 廃止日の設定
    /// </summary>
    /// <param name="abolishedOn">廃止日</param>
    /// <exception cref="ArgumentNullException"><paramref name="abolishedOn"/> が <see langword="null"/> の場合</exception>
    public void Abolish(AbolishedOn abolishedOn)
    {
        ArgumentNullException.ThrowIfNull(abolishedOn);
        AbolishedOn = abolishedOn;
    }

    /// <summary>
    /// 廃止の取り消し
    /// </summary>
    public void RestoreFromAbolithing()
    {
        AbolishedOn = AbolishedOn.Unset();
    }

    /// <summary>
    /// デバッグ用の文字列表現の取得
    /// </summary>
    /// <returns><c>Department(RowId=…, Code=…, Name=…)</c> 形式の文字列。画面表示やデータの解析には使用禁止</returns>
    public override string ToString()
        => $"Department(RowId={RowId.Value}, Code={DeptCode}, Name={Name})";
}
