namespace SupportAdvance.Contexts.Employee.Infrastructure.DataAccess.Models;

using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Employee をデータベースで永続化するモデル
/// 【責務】DB テーブル m_employees との O/R マッピング
/// 【特徴】すべてのプロパティはプリミティブ型（ValueObject の分解値）
/// </summary>
[Table("m_employees")]
public class EmployeeDbModel
{
    // ==================== 監査カラム（8つ、必須） ====================

    /// <summary>
    /// 行ID（主キー、DB 自動採番）
    /// </summary>
    [Column("row_id")]
    public long RowId { get; set; }

    /// <summary>
    /// 行バージョン（楽観ロック用、DB 自動更新）
    /// </summary>
    [Column("row_version")]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// 作成日時
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 作成者RowId
    /// </summary>
    [Column("created_by")]
    public long CreatedBy { get; set; }

    /// <summary>
    /// 更新日時（NULL 許可）
    /// </summary>
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 更新者RowId（NULL 許可）
    /// </summary>
    [Column("updated_by")]
    public long? UpdatedBy { get; set; }

    /// <summary>
    /// 削除日時（論理削除用、NULL 許可）
    /// </summary>
    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 削除者RowId（NULL 許可）
    /// </summary>
    [Column("deleted_by")]
    public long? DeletedBy { get; set; }

    // ==================== ビジネスカラム ====================

    /// <summary>
    /// 従業員ID（1001以上、集約根ID）
    /// </summary>
    [Column("employee_id")]
    public int EmployeeId { get; set; }

    /// <summary>
    /// 雇用終了日（従業員でなくなった日、NULL許可）
    /// </summary>
    [Column("retired_at")]
    public DateTime? RetiredAt { get; set; }
}
