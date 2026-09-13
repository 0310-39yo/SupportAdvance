using System.ComponentModel.DataAnnotations.Schema;

namespace SupportAdvance.Contexts.Employee.Infrastructure.Models;

/// <summary>
/// Employee テーブルマッピングモデル
///
/// 【テーブル】m_employees
/// 【責務】DB スキーマとの ORM マッピング、プリミティブ型で保持
/// 【特徴】従業員のビジネス属性のみ（個人情報は m_persons に分離、監査カラムは Repository で自動管理）
/// 【1:1 関係】Person は m_persons テーブル（employee_row_id FK）で1:1に対応
/// </summary>
[Table("m_employees")]
public class EmployeeDbModel
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
    /// ビジネス区分（M/D/C のいずれか）
    /// 【対応カラム】biz_division
    /// 【値】M=正社員, D=派遣, C=契約
    /// </summary>
    [Column("biz_division")]
    public string BizDivision { get; set; } = string.Empty;

    /// <summary>
    /// ビジネスID（従業員番号、1001以上）
    /// 【対応カラム】biz_id
    /// 【特徴】区分ごとに有効範囲が異なる
    /// </summary>
    [Column("biz_id")]
    public int BizId { get; set; }

    /// <summary>
    /// 退職日（在職状況を示す）
    /// 【対応カラム】retired_on
    /// 【制約】NULL許可（現職時は NULL）
    /// </summary>
    [Column("retired_on")]
    public DateTime? RetiredOn { get; set; }

    /// <summary>
    /// 作成日時（LocalDateTime/JST）
    /// 【対応カラム】created_at
    /// 【責務】監査ログ（作成日時記録）
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 作成者RowId
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
    /// 更新者RowId
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
    /// 削除者RowId
    /// 【対応カラム】deleted_by
    /// 【制約】NULL許可（削除されていない場合）
    /// 【責務】監査ログ（誰が削除したか）
    /// </summary>
    [Column("deleted_by")]
    public long? DeletedBy { get; set; }
}
