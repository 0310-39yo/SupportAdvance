namespace SupportAdvance.Contexts.Employee.Infrastructure.Models;

using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// DepartmentMembership テーブルマッピングモデル
///
/// 【テーブル】m_department_memberships
/// 【責務】DB スキーマとの ORM マッピング、プリミティブ型で保持
/// 【特徴】ビジネスカラムのみ（監査カラムは Repository で自動管理）
/// </summary>
[Table("m_department_memberships")]
public class DepartmentMembershipDbModel
{
    /// <summary>
    /// データベース行ID（主キー、Sequence自動採番）
    /// 【対応カラム】row_id
    /// </summary>
    [Column("row_id")]
    public long RowId { get; set; }

    /// <summary>
    /// 楽観ロックタイムスタンプ
    /// 【対応カラム】row_version
    /// 【責務】concurrency control（更新時に競合検出）
    /// 【重要】SQL Server の timestamp は自動管理のため、RepoDb の fields パラメータで INSERT/UPDATE から除外
    /// </summary>
    [Column("row_version")]
    public byte[]? RowVersion { get; set; }

    /// <summary>
    /// 従業員行ID（外部参照）
    /// 【対応カラム】employee_row_id
    /// 【制約】FK → m_employees
    /// </summary>
    [Column("employee_row_id")]
    public long EmployeeRowId { get; set; }

    /// <summary>
    /// 部署行ID（外部参照）
    /// 【対応カラム】department_row_id
    /// 【制約】FK → m_departments
    /// </summary>
    [Column("department_row_id")]
    public long DepartmentRowId { get; set; }

    /// <summary>
    /// 主部署フラグ
    /// 【対応カラム】is_primary
    /// 【値】true=主部署, false=副部署
    /// </summary>
    [Column("is_primary")]
    public bool IsPrimary { get; set; }

    /// <summary>
    /// 異動終了日（無期限の場合は NULL）
    /// 【対応カラム】end_on
    /// 【制約】NULL許可（継続中は NULL）
    /// </summary>
    [Column("end_on")]
    public DateTime? EndOn { get; set; }

    /// <summary>
    /// 部署名（表示用、JOINで取得）
    /// 【対応カラム】department_name
    /// </summary>
    [Column("department_name")]
    public string? DepartmentName { get; set; }
}
