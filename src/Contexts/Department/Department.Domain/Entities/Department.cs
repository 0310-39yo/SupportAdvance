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
    /// <param name="rowId">採番済みの部署の行ID</param>
    /// <param name="deptCode">部署コード</param>
    /// <param name="name">部署名</param>
    /// <param name="level">階層レベル</param>
    /// <param name="parentId">親部署の行ID。<see langword="null"/> の場合は親なし（<see cref="ParentDepartmentRowId.Unset"/>）</param>
    /// <param name="managerId">管理者の従業員行ID。<see langword="null"/> の場合は未設定（<see cref="ManagerEmployeeRowId.Unset"/>）</param>
    /// <param name="abolishedOn">廃止日。<see langword="null"/> の場合は未廃止（<see cref="AbolishedOn.Unset"/>）</param>
    /// <returns>生成した部署（<c>RowVersion</c> は空。リポジトリは空の場合を新規作成として扱う）</returns>
    /// <exception cref="ArgumentNullException"><paramref name="deptCode"/> または <paramref name="name"/> が <see langword="null"/> の場合</exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> が空文字の場合</exception>
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
    /// </remarks>
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
    /// <param name="newParentId">新しい親部署の行ID。Unset の場合はトップレベル化</param>
    /// <exception cref="InvalidOperationException"><paramref name="newParentId"/> が自分自身の場合</exception>
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
    /// <param name="newManagerId">新しい管理者の従業員行ID。Unset の場合は管理者なし</param>
    public void UpdateManager(ManagerEmployeeRowId newManagerId)
    {
        ManagerId = newManagerId;
    }

    /// <summary>
    /// 廃止日を設定する
    /// </summary>
    /// <param name="abolishedOn">廃止日</param>
    /// <exception cref="ArgumentNullException"><paramref name="abolishedOn"/> が <see langword="null"/> の場合</exception>
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
    /// <returns><c>Department(RowId=…, Code=…, Name=…)</c> 形式のデバッグ用文字列</returns>
    public override string ToString()
        => $"Department(RowId={RowId.Value}, Code={DeptCode}, Name={Name})";
}
