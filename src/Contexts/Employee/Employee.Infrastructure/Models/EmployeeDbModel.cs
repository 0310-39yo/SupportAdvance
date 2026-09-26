using System.ComponentModel.DataAnnotations.Schema;

namespace SupportAdvance.Contexts.Employee.Infrastructure.Models;

/// <summary>
/// Employee テーブルマッピングモデル
/// </summary>
/// <remarks>
/// <para>【テーブル】m_employees</para>
/// <para>【責務】DB スキーマとの ORM マッピング、プリミティブ型で保持</para>
/// <para>【特徴】従業員のビジネス属性のみ（個人情報は m_persons に分離、監査カラムは Repository で自動管理）</para>
/// <para>【1:1 関係】Person は m_persons テーブル（employee_row_id FK）で1:1に対応</para>
/// </remarks>
[Table("m_employees")]
public class EmployeeDbModel
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
    /// ビジネス区分（M/D/C のいずれか）
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】biz_division</para>
    /// <para>【値】M=正社員, D=派遣, C=契約</para>
    /// </remarks>
    [Column("biz_division")]
    public string BizDivision { get; set; } = string.Empty;

    /// <summary>
    /// ビジネスID（従業員番号、1001以上）
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】biz_id</para>
    /// <para>【特徴】区分ごとの有効範囲の相違</para>
    /// </remarks>
    [Column("biz_id")]
    public int BizId { get; set; }

    /// <summary>
    /// 退職日（在職状況を示す）
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】retired_on</para>
    /// <para>【制約】NULL許可（現職時は NULL）</para>
    /// </remarks>
    [Column("retired_on")]
    public DateTime? RetiredOn { get; set; }

    /// <summary>
    /// 作成日時（LocalDateTime/JST）
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】created_at</para>
    /// <para>【責務】監査ログ（作成日時記録）</para>
    /// </remarks>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 作成者RowId
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】created_by</para>
    /// <para>【責務】監査ログ（誰が作成したか）</para>
    /// </remarks>
    [Column("created_by")]
    public long CreatedBy { get; set; }

    /// <summary>
    /// 更新日時（LocalDateTime/JST）
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】updated_at</para>
    /// <para>【制約】NULL許可（未更新時）</para>
    /// <para>【責務】監査ログ（最終更新日時）</para>
    /// </remarks>
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 更新者RowId
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】updated_by</para>
    /// <para>【制約】NULL許可（未更新時）</para>
    /// <para>【責務】監査ログ（最後に誰が更新したか）</para>
    /// </remarks>
    [Column("updated_by")]
    public long? UpdatedBy { get; set; }

    /// <summary>
    /// 削除日時（LocalDateTime/JST）
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】deleted_at</para>
    /// <para>【制約】NULL許可（削除されていない場合）</para>
    /// <para>【責務】論理削除フラグ、ソフトデリート用</para>
    /// </remarks>
    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 削除者RowId
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】deleted_by</para>
    /// <para>【制約】NULL許可（削除されていない場合）</para>
    /// <para>【責務】監査ログ（誰が削除したか）</para>
    /// </remarks>
    [Column("deleted_by")]
    public long? DeletedBy { get; set; }
}
