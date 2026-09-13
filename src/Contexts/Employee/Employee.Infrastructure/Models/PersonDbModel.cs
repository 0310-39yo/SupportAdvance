using System.ComponentModel.DataAnnotations.Schema;

namespace SupportAdvance.Contexts.Employee.Infrastructure.Models;

/// <summary>
/// Person テーブルマッピングモデル
///
/// 【テーブル】m_persons
/// 【責務】DB スキーマとの ORM マッピング、プリミティブ型で保持
/// 【特徴】個人基本情報（氏名・カナ氏名）のみ、監査カラムは Repository で自動管理
/// 【1:1 関係】Employee との 1:1 マッピング（employee_row_id FK で連携）
/// </summary>
[Table("m_persons")]
public class PersonDbModel
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
    /// </summary>
    [Column("row_version")]
    public byte[] RowVersion { get; set; } = [];

    /// <summary>
    /// 従業員RowId（m_employees の row_id）
    /// 【対応カラム】employee_row_id
    /// 【関係】Person が属する Employee を特定（1:1 関係）
    /// </summary>
    [Column("employee_row_id")]
    public long EmployeeRowId { get; set; }

    /// <summary>
    /// 姓
    /// 【対応カラム】last_name
    /// </summary>
    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// 名
    /// 【対応カラム】first_name
    /// </summary>
    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// 姓（カナ）
    /// 【対応カラム】last_name_kana
    /// </summary>
    [Column("last_name_kana")]
    public string LastNameKana { get; set; } = string.Empty;

    /// <summary>
    /// 名（カナ）
    /// 【対応カラム】first_name_kana
    /// </summary>
    [Column("first_name_kana")]
    public string FirstNameKana { get; set; } = string.Empty;

    /// <summary>
    /// 作成日時（LocalDateTime/JST）
    /// 【対応カラム】created_at
    /// 【責務】監査ログ（作成日時記録）
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 作成者RowId（m_persons の row_id）
    /// 【対応カラム】created_by
    /// 【責務】監査ログ（誰が作成したか）
    /// </summary>
    [Column("created_by")]
    public long CreatedBy { get; set; }

    /// <summary>
    /// 更新日時（LocalDateTime/JST）
    /// 【対応カラム】updated_at
    /// 【制約】NULL許可（未更新時）
    /// 【責務】監査ログ（最終更新日時）
    /// </summary>
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 更新者RowId（m_persons の row_id）
    /// 【対応カラム】updated_by
    /// 【制約】NULL許可（未更新時）
    /// 【責務】監査ログ（最後に誰が更新したか）
    /// </summary>
    [Column("updated_by")]
    public long? UpdatedBy { get; set; }

    /// <summary>
    /// 削除日時（LocalDateTime/JST）
    /// 【対応カラム】deleted_at
    /// 【制約】NULL許可（削除されていない場合）
    /// 【責務】論理削除フラグ、ソフトデリート用
    /// </summary>
    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 削除者RowId（m_persons の row_id）
    /// 【対応カラム】deleted_by
    /// 【制約】NULL許可（削除されていない場合）
    /// 【責務】監査ログ（誰が削除したか）
    /// </summary>
    [Column("deleted_by")]
    public long? DeletedBy { get; set; }
}
