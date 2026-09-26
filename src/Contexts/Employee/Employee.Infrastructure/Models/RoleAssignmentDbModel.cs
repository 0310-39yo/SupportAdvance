namespace SupportAdvance.Contexts.Employee.Infrastructure.Models;

/// <summary>
/// RoleAssignment テーブルマッピングモデル
/// </summary>
/// <remarks>
/// <para>【テーブル】t_role_assignments</para>
/// <para>【責務】DB スキーマとの ORM マッピング、プリミティブ型で保持</para>
/// </remarks>
public class RoleAssignmentDbModel
{
    /// <summary>
    /// データベース行ID（主キー）
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】row_id</para>
    /// </remarks>
    public long RowId { get; set; }

    /// <summary>
    /// 従業員行ID（外部参照）
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】employee_row_id</para>
    /// <para>【制約】FK → t_employees</para>
    /// </remarks>
    public long EmployeeRowId { get; set; }

    /// <summary>
    /// ロールコード
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】role_code</para>
    /// </remarks>
    public string RoleCode { get; set; } = string.Empty;

    /// <summary>
    /// 有効開始日時
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】effective_date</para>
    /// </remarks>
    public DateTime EffectiveDate { get; set; }

    /// <summary>
    /// 有効終了日時（無期限の場合は NULL）
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】expiration_date</para>
    /// </remarks>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】created_at</para>
    /// </remarks>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 作成者従業員行ID
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】created_by</para>
    /// </remarks>
    public long CreatedBy { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】updated_at</para>
    /// </remarks>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 更新者従業員行ID
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】updated_by</para>
    /// </remarks>
    public long? UpdatedBy { get; set; }

    /// <summary>
    /// 削除日時（論理削除フラグ）
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】deleted_at</para>
    /// </remarks>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 削除者従業員行ID
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】deleted_by</para>
    /// </remarks>
    public long? DeletedBy { get; set; }

    /// <summary>
    /// 楽観ロック用タイムスタンプ
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】row_version</para>
    /// </remarks>
    public byte[]? RowVersion { get; set; }

    /// <summary>
    /// 親エンティティ参照（ナビゲーションプロパティ）
    /// </summary>
    public EmployeeDbModel? Employee { get; set; }
}
