using System.ComponentModel.DataAnnotations.Schema;

namespace SupportAdvance.Contexts.Authentication.Infrastructure.DbModels;

/// <summary>
/// ローカル認証情報マスター データベースモデル
/// </summary>
/// <remarks>
/// <para>【責務】</para>
/// <list type="bullet">
/// <item><description>m_login_credentials テーブルのマッピング</description></item>
/// <item><description>ローカル認証用の認証情報（ログインID、パスワードハッシュ）を保持</description></item>
/// </list>
/// <para>【注意】</para>
/// <list type="bullet">
/// <item><description>監査フィールド（CreatedAt/UpdatedAt/DeletedAt）は DateTime プリミティブ型</description></item>
/// <item><description>パスワードハッシュは Domain層を通さず、直接保存・比較（検証ロジック）</description></item>
/// </list>
/// </remarks>
[Table("m_login_credentials")]
public class LoginCredentialsDbModel
{
    /// <summary>
    /// 認証情報マスター行ID（主キー）
    /// </summary>
    [Column("row_id")]
    public long RowId { get; set; }

    /// <summary>
    /// 紐づいた従業員行ID
    /// </summary>
    /// <remarks>
    /// <para>【制約】NOT NULL、FK → m_employees(row_id)</para>
    /// <para>【意味】社外から m_login_credentials 経由でローカル認証ログインする人物が、社内の Employee 情報上では誰にあたるかを示すマッピング。すなわち「社外からログインした人物」と「社内の Employee として認識される人物」は、この値（m_employees.row_id）を介して同一人物であることの保証。認証成功後の権限判定・監査記録（AuthorityRowId）は常にこの EmployeeRowId に対して実施</para>
    /// </remarks>
    [Column("mapping_employee_row_id")]
    public long MappingEmployeeRowId { get; set; }

    /// <summary>
    /// ログインID（従業員番号など）
    /// </summary>
    /// <remarks>
    /// <para>【制約】NOT NULL、最大50文字</para>
    /// </remarks>
    [Column("login_id")]
    public string LoginId { get; set; } = string.Empty;

    /// <summary>
    /// パスワードハッシュ値
    /// </summary>
    /// <remarks>
    /// <para>【制約】NOT NULL、最大255文字</para>
    /// </remarks>
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 認証情報が有効か（true=有効、false=無効）
    /// </summary>
    [Column("is_active")]
    public bool IsActive { get; set; }

    /// <summary>
    /// 最後のログイン日時（NULL で「未ログイン」）
    /// </summary>
    [Column("last_login_at")]
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// 楽観ロック用タイムスタンプ
    /// </summary>
    [Column("row_version")]
    public byte[] RowVersion { get; set; } = [];

    /// <summary>
    /// 作成日時（JST）
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 作成者の従業員行ID
    /// </summary>
    [Column("created_by")]
    public long CreatedBy { get; set; }

    /// <summary>
    /// 更新日時（JST）（NULL で「未更新」）
    /// </summary>
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 更新者の従業員行ID（NULL で「未更新」）
    /// </summary>
    [Column("updated_by")]
    public long? UpdatedBy { get; set; }

    /// <summary>
    /// 削除日時（JST）（NULL で「削除されていない」）
    /// </summary>
    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 削除者の従業員行ID（NULL で「削除されていない」）
    /// </summary>
    [Column("deleted_by")]
    public long? DeletedBy { get; set; }
}
