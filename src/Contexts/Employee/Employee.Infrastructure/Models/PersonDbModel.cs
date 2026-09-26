using System.ComponentModel.DataAnnotations.Schema;

namespace SupportAdvance.Contexts.Employee.Infrastructure.Models;

/// <summary>
/// Person テーブルマッピングモデル
/// </summary>
/// <remarks>
/// <para>【テーブル】m_persons</para>
/// <para>【責務】DB スキーマとの ORM マッピング、プリミティブ型で保持</para>
/// <para>【特徴】個人基本情報（氏名・カナ氏名）のみ、監査カラムは Repository で自動管理</para>
/// <para>【1:1 関係】Employee との 1:1 マッピング（employee_row_id FK で連携）</para>
/// </remarks>
[Table("m_persons")]
public class PersonDbModel
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
    /// 従業員RowId（m_employees の row_id）
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】employee_row_id</para>
    /// <para>【関係】Person が属する Employee を特定（1:1 関係）</para>
    /// </remarks>
    [Column("employee_row_id")]
    public long EmployeeRowId { get; set; }

    /// <summary>
    /// 姓
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】last_name</para>
    /// </remarks>
    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// 名
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】first_name</para>
    /// </remarks>
    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// 姓（カナ）
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】last_name_kana</para>
    /// </remarks>
    [Column("last_name_kana")]
    public string LastNameKana { get; set; } = string.Empty;

    /// <summary>
    /// 名（カナ）
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】first_name_kana</para>
    /// </remarks>
    [Column("first_name_kana")]
    public string FirstNameKana { get; set; } = string.Empty;

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
    /// 作成者RowId（m_persons の row_id）
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
    /// 更新者RowId（m_persons の row_id）
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
    /// 削除者RowId（m_persons の row_id）
    /// </summary>
    /// <remarks>
    /// <para>【対応カラム】deleted_by</para>
    /// <para>【制約】NULL許可（削除されていない場合）</para>
    /// <para>【責務】監査ログ（誰が削除したか）</para>
    /// </remarks>
    [Column("deleted_by")]
    public long? DeletedBy { get; set; }
}
