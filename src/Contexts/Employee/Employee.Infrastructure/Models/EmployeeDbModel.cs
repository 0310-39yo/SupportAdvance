namespace SupportAdvance.Contexts.Employee.Infrastructure.Models;

/// <summary>
/// Employee テーブルマッピングモデル
///
/// 【テーブル】t_employees
/// 【責務】DB スキーマとの ORM マッピング、プリミティブ型で保持
/// 【特徴】DateTime プリミティブ型（LocalDateTime は Application/Domain層で使用）
/// </summary>
public class EmployeeDbModel
{
    /// <summary>
    /// データベース行ID（主キー、Sequence自動採番）
    /// 【対応カラム】row_id
    /// </summary>
    public long RowId { get; set; }


    /// <summary>
    /// 従業員コード区分（M/T/C のいずれか）
    /// 【対応カラム】employee_code_division
    /// 【値】M=正社員, T=派遣, C=契約
    /// </summary>
    public string EmployeeCodeDivision { get; set; } = string.Empty;

    /// <summary>
    /// 従業員コード番号（1001-9999 の範囲）
    /// 【対応カラム】employee_code_number
    /// </summary>
    public int EmployeeCodeNumber { get; set; }

    /// <summary>
    /// 人事マスタ行ID（m_persons.row_id への外部参照）
    /// 【対応カラム】person_row_id
    /// 【制約】NOT NULL, FK → m_persons
    /// </summary>
    public long PersonRowId { get; set; }

    /// <summary>
    /// 作成日時（JST、DateTime プリミティブ型）
    /// 【対応カラム】created_at
    /// 【制約】NOT NULL
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 作成者従業員行ID（m_persons.row_id への外部参照）
    /// 【対応カラム】created_by
    /// 【制約】NOT NULL, FK → m_persons
    /// </summary>
    public long CreatedBy { get; set; }

    /// <summary>
    /// 更新日時（JST、DateTime プリミティブ型）
    /// 【対応カラム】updated_at
    /// 【制約】NULL許可（未更新）
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 更新者従業員行ID（m_persons.row_id への外部参照）
    /// 【対応カラム】updated_by
    /// 【制約】NULL許可、FK → m_persons
    /// </summary>
    public long? UpdatedBy { get; set; }

    /// <summary>
    /// 削除日時（論理削除フラグ、DateTime プリミティブ型）
    /// 【対応カラム】deleted_at
    /// 【制約】NULL許可（IS NULL で有効行フィルタ）
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 削除者従業員行ID（m_persons.row_id への外部参照）
    /// 【対応カラム】deleted_by
    /// 【制約】NULL許可、FK → m_persons
    /// </summary>
    public long? DeletedBy { get; set; }

    /// <summary>
    /// 退職日（論理削除ではなく、在職状況を示す）
    /// 【対応カラム】retired_on
    /// 【制約】NULL許可（現職時は NULL）
    /// </summary>
    public DateTime? RetiredOn { get; set; }

    /// <summary>
    /// 楽観ロック用タイムスタンプ
    /// 【対応カラム】row_version
    /// 【制約】NOT NULL, ROWVERSION
    /// </summary>
    public byte[]? RowVersion { get; set; }

    /// <summary>
    /// 子エンティティ: 部署メンバーシップ
    /// 【リレーション】1:N（Employee:DepartmentMembership）
    /// </summary>
    public List<DepartmentMembershipDbModel> DepartmentMemberships { get; set; } = new();

    /// <summary>
    /// 子エンティティ: ロール割り当て
    /// 【リレーション】1:N（Employee:RoleAssignment）
    /// </summary>
    public List<RoleAssignmentDbModel> RoleAssignments { get; set; } = new();

    /// <summary>
    /// 子エンティティ: 権限割り当て
    /// 【リレーション】1:N（Employee:PermissionAssignment）
    /// </summary>
    public List<PermissionAssignmentDbModel> PermissionAssignments { get; set; } = new();
}
