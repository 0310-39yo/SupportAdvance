using System.ComponentModel.DataAnnotations.Schema;

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
[Table("m_departments")]
public class DepartmentDbModel
{
    /// <summary>
    /// 行ID（主キー）
    /// </summary>
    [Column("row_id")]
    public long RowId { get; set; }

    /// <summary>
    /// 部署コード（4文字、一意）
    /// </summary>
    [Column("department_code")]
    public string DepartmentCode { get; set; } = string.Empty;

    /// <summary>
    /// 部署名
    /// </summary>
    [Column("department_name")]
    public string DepartmentName { get; set; } = string.Empty;

    /// <summary>
    /// 階層レベル（0-4）
    /// </summary>
    [Column("hierarchy_level")]
    public int HierarchyLevel { get; set; }

    /// <summary>
    /// 親部署の行ID（NULL で「トップレベル」）
    /// </summary>
    [Column("parent_department_row_id")]
    public long? ParentDepartmentRowId { get; set; }

    /// <summary>
    /// 部署管理者の従業員行ID（NULL で「未指定」）
    /// </summary>
    [Column("manager_employee_row_id")]
    public long? ManagerEmployeeRowId { get; set; }

    /// <summary>
    /// 廃止日（NULL で「廃止されていない」）
    /// </summary>
    [Column("abolished_on")]
    public DateTime? AbolishedOn { get; set; }

    /// <summary>
    /// 楽観ロック用タイムスタンプ
    /// 【重要】SQL Server の timestamp は自動管理のため、RepoDb の fields パラメータで INSERT/UPDATE から除外
    /// </summary>
    [Column("row_version")]
    public byte[]? RowVersion { get; set; }

    /// <summary>
    /// 作成日時（JST）
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 作成者の従業員行ID
    /// </summary>
    [Column("created_by")]
    public long CreatedBy { get; set; }

    /// <summary>
    /// 更新日時（JST）（NULL で「未更新」）
    /// </summary>
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 更新者の従業員行ID（NULL で「未更新」）
    /// </summary>
    [Column("updated_by")]
    public long? UpdatedBy { get; set; }

    /// <summary>
    /// 論理削除日時（NULL で「未削除」）
    /// </summary>
    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 論理削除者の従業員行ID（NULL で「未削除」）
    /// </summary>
    [Column("deleted_by")]
    public long? DeletedBy { get; set; }
}
