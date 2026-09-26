namespace SupportAdvance.Contexts.Employee.Infrastructure.Models;

using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// DepartmentMembership テーブルマッピングモデル
/// </summary>
/// <remarks>
/// <para>【テーブル】m_department_memberships</para>
/// <para>【責務】DB スキーマとの ORM マッピング、プリミティブ型で保持</para>
/// <para>【特徴】ビジネスカラムのみ（監査カラムは Repository で自動管理）</para>
/// </remarks>
[Table("m_department_memberships")]
public class DepartmentMembershipDbModel
{
    /// <summary>
    /// データベース行ID（主キー、Sequence自動採番）
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】row_id</para>
    /// </remarks>
    [Column("row_id")]
    public long RowId { get; set; }

    /// <summary>
    /// 楽観ロックタイムスタンプ
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】row_version</para>
    /// <para>【責務】concurrency control（更新時に競合検出）</para>
    /// <para>【重要】SQL Server の timestamp は自動管理のため、RepoDb の fields パラメータで INSERT/UPDATE から除外</para>
    /// </remarks>
    [Column("row_version")]
    public byte[]? RowVersion { get; set; }

    /// <summary>
    /// 従業員行ID（外部参照）
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】employee_row_id</para>
    /// <para>【制約】FK → m_employees</para>
    /// </remarks>
    [Column("employee_row_id")]
    public long EmployeeRowId { get; set; }

    /// <summary>
    /// 部署行ID（外部参照）
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】department_row_id</para>
    /// <para>【制約】FK → m_departments</para>
    /// </remarks>
    [Column("department_row_id")]
    public long DepartmentRowId { get; set; }

    /// <summary>
    /// 主部署フラグ
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】is_primary</para>
    /// <para>【値】true=主部署, false=副部署</para>
    /// </remarks>
    [Column("is_primary")]
    public bool IsPrimary { get; set; }

    /// <summary>
    /// 異動終了日（無期限の場合は NULL）
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】end_on</para>
    /// <para>【制約】NULL許可（継続中は NULL）</para>
    /// </remarks>
    [Column("end_on")]
    public DateTime? EndOn { get; set; }

    /// <summary>
    /// 部署名（表示用、JOINで取得）
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】department_name</para>
    /// </remarks>
    [Column("department_name")]
    public string? DepartmentName { get; set; }
}
