namespace SupportAdvance.Contexts.Department.Infrastructure.DbModels;

/// <summary>
/// 部署データベースモデル
///
/// 【責務】
///   - m_departments テーブルのマッピング
///   - プリミティブ型のデータベーススキーマ表現
/// 【注意】
///   - 監査フィールド（CreatedAt/UpdatedAt/DeletedAt）は DateTime プリミティブ型
///   - ビジネスフィールドも DateTime（LocalDateTime ↔ DateTime は Mapper で変換）
/// </summary>
public class DepartmentDbModel
{
    /// <summary>
    /// 行ID（主キー）
    /// </summary>
    public long RowId { get; set; }

    /// <summary>
    /// 部署コード（4文字、一意）
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 部署名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 階層レベル（0-4）
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 親部署の行ID（NULL で「トップレベル」）
    /// </summary>
    public long? ParentDepartmentRowId { get; set; }

    /// <summary>
    /// 部署管理者の従業員行ID（NULL で「未指定」）
    /// </summary>
    public long? ManagerEmployeeRowId { get; set; }

    /// <summary>
    /// 廃止日（NULL で「廃止されていない」）
    /// </summary>
    public DateTime? AbolishedOn { get; set; }

    /// <summary>
    /// 楽観ロック用タイムスタンプ
    /// </summary>
    public byte[] RowVersion { get; set; } = [];

    /// <summary>
    /// 作成日時（JST）
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 作成者の従業員行ID
    /// </summary>
    public long CreatedBy { get; set; }

    /// <summary>
    /// 更新日時（JST）（NULL で「未更新」）
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 更新者の従業員行ID（NULL で「未更新」）
    /// </summary>
    public long? UpdatedBy { get; set; }

    /// <summary>
    /// 論理削除日時（NULL で「未削除」）
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 論理削除者の従業員行ID（NULL で「未削除」）
    /// </summary>
    public long? DeletedBy { get; set; }
}
