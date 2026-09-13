using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Department.Domain.ValueObjects;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Department.Domain.Entities;

/// <summary>
/// 部署を表すドメインエンティティ（集約根）
///
/// 【集約ID】DepartmentRowId（long ベース）
/// 【責務】部署のマスターデータ管理（コード、階層、親部署、管理者、廃止日）
/// 【不変条件】
///   - コード一意性（同じコードの部署は複数存在しない）
///   - 階層整合性（親部署の階層レベルより低い）
///   - 循環参照禁止（親部署チェーンにループがない）
/// 【ライフサイクル】作成～廃止までの全期間をトラッキング
/// 【Application層インターフェース】IDepartment を実装（Context間での参照用）
/// </summary>
public sealed class Department : AggregateRoot<DepartmentRowId>, IDepartment
{
    /// <summary>
    /// 楽観ロックタイムスタンプ（concurrency control 用）
    /// 【責務】DB更新時の競合検出
    /// 【管理】Repository で更新時に新しい値で上書きされる
    /// </summary>
    public byte[] RowVersion { get; internal set; } = [];

    /// <summary>
    /// 部署コード（ビジネスID、4文字固定）
    /// </summary>
    public DepartmentCode DeptCode { get; private set; }

    /// <summary>
    /// 部署名
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 階層レベル（0:会社, 1:本部, 2:部, 3:グループ, 4:チーム）
    /// </summary>
    public HierarchyLevel Level { get; private set; }

    /// <summary>
    /// 親部署の行ID（Unset で「トップレベル」を表現）
    /// </summary>
    public ParentDepartmentRowId ParentId { get; private set; }

    /// <summary>
    /// 部署管理者の従業員行ID（Unset で「管理者未指定」を表現）
    /// </summary>
    public ManagerEmployeeRowId ManagerId { get; private set; }

    /// <summary>
    /// 廃止日（Unset で「廃止されていない」を表現）
    /// </summary>
    public AbolishedOn AbolishedOn { get; private set; }

    /// <summary>
    /// 指定されたプロパティから Department を生成する（プライベートコンストラクタ）
    /// </summary>
    private Department(
        DepartmentRowId rowId,
        DepartmentCode deptCode,
        string name,
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
    /// 新規部署を生成する（ファクトリメソッド）
    /// 【責務】Application/Infrastructure 層での Department 生成
    /// 【パラメータ】rowId は事前採番済み、parentId/managerId/abolishedOn はオプション
    /// </summary>
    public static Department Create(
        DepartmentRowId rowId,
        DepartmentCode deptCode,
        string name,
        HierarchyLevel level,
        ParentDepartmentRowId? parentId = null,
        ManagerEmployeeRowId? managerId = null,
        AbolishedOn? abolishedOn = null)
    {
        ArgumentNullException.ThrowIfNull(deptCode);
        ArgumentNullException.ThrowIfNullOrEmpty(name);

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
    /// DB から読み込んだ値から Department を復元する（ファクトリメソッド）
    /// 【責務】DB の プリミティブ型 → Domain Entity に変換
    /// 【パラメータ】rowVersion は楽観ロック用（更新時に競合検出）
    /// </summary>
    public static Department Reconstruct(
        DepartmentRowId rowId,
        DepartmentCode deptCode,
        string name,
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
    /// 部署が現在有効か判定する（廃止されていないか）
    /// </summary>
    /// <param name="asOf">判定日時（JST）</param>
    /// <returns>有効な場合 true</returns>
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
    /// 親部署を更新する（不変条件チェック付き）
    /// 【責務】親部署の妥当性を検証（循環参照チェックなど）
    /// 【注意】実装では Repository が親部署の存在確認を担当
    /// </summary>
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
    /// 管理者を更新する
    /// </summary>
    public void UpdateManager(ManagerEmployeeRowId newManagerId)
    {
        ManagerId = newManagerId;
    }

    /// <summary>
    /// 廃止日を設定する
    /// </summary>
    public void Abolish(AbolishedOn abolishedOn)
    {
        ArgumentNullException.ThrowIfNull(abolishedOn);
        AbolishedOn = abolishedOn;
    }

    /// <summary>
    /// 廃止を取り消す
    /// </summary>
    public void RestoreFromAbolithing()
    {
        AbolishedOn = AbolishedOn.Unset();
    }

    /// <summary>
    /// 文字列表現を取得する
    /// </summary>
    public override string ToString()
        => $"Department(RowId={RowId.Value}, Code={DeptCode}, Name={Name})";
}
