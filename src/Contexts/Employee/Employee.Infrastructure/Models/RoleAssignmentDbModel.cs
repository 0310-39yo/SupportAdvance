namespace SupportAdvance.Contexts.Employee.Infrastructure.Models;

/// <summary>
/// RoleAssignment テーブルマッピングモデル
///
/// 【テーブル】t_role_assignments
/// 【責務】DB スキーマとの ORM マッピング、プリミティブ型で保持
/// </summary>
public class RoleAssignmentDbModel
{
    /// <summary>
    /// データベース行ID（主キー）
    /// 【対応カラム】row_id
    /// </summary>
    public long RowId { get; set; }

    /// <summary>
    /// ロール割り当て集約根ID（GUID）
    /// 【対応カラム】role_assignment_id
    /// </summary>
    public Guid RoleAssignmentId { get; set; }

    /// <summary>
    /// 従業員行ID（外部参照）
    /// 【対応カラム】employee_row_id
    /// 【制約】FK → t_employees
    /// </summary>
    public long EmployeeRowId { get; set; }

    /// <summary>
    /// ロールコード
    /// 【対応カラム】role_code
    /// </summary>
    public string RoleCode { get; set; } = string.Empty;

    /// <summary>
    /// 有効開始日時
    /// 【対応カラム】effective_date
    /// </summary>
    public DateTime EffectiveDate { get; set; }

    /// <summary>
    /// 有効終了日時（無期限の場合は NULL）
    /// 【対応カラム】expiration_date
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// 作成日時
    /// 【対応カラム】created_at
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 作成者従業員行ID
    /// 【対応カラム】created_by
    /// </summary>
    public long CreatedBy { get; set; }

    /// <summary>
    /// 更新日時
    /// 【対応カラム】updated_at
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 更新者従業員行ID
    /// 【対応カラム】updated_by
    /// </summary>
    public long? UpdatedBy { get; set; }

    /// <summary>
    /// 削除日時（論理削除フラグ）
    /// 【対応カラム】deleted_at
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 削除者従業員行ID
    /// 【対応カラム】deleted_by
    /// </summary>
    public long? DeletedBy { get; set; }

    /// <summary>
    /// 楽観ロック用タイムスタンプ
    /// 【対応カラム】row_version
    /// </summary>
    public byte[]? RowVersion { get; set; }

    /// <summary>
    /// 親エンティティ参照（ナビゲーションプロパティ）
    /// </summary>
    public EmployeeDbModel? Employee { get; set; }
}
