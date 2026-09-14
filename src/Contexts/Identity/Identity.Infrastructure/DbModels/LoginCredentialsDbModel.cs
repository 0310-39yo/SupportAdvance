using System.ComponentModel.DataAnnotations.Schema;

namespace SupportAdvance.Contexts.Identity.Infrastructure.DbModels;

/// <summary>
/// ローカル認証情報マスター データベースモデル
///
/// 【責務】
///   - m_login_credentials テーブルのマッピング
///   - ローカル認証用の認証情報（ログインID、パスワードハッシュ）を保持
/// 【注意】
///   - 監査フィールド（CreatedAt/UpdatedAt/DeletedAt）は DateTime プリミティブ型
///   - パスワードハッシュは Domain層を通さず、直接保存・比較（検証ロジック）
/// </summary>
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
    /// 【制約】NOT NULL、FK → m_employees(row_id)
    /// </summary>
    [Column("mapping_employee_row_id")]
    public long MappingEmployeeRowId { get; set; }

    /// <summary>
    /// ログインID（従業員番号など）
    /// 【制約】NOT NULL、最大50文字
    /// </summary>
    [Column("login_id")]
    public string LoginId { get; set; } = string.Empty;

    /// <summary>
    /// パスワードハッシュ値
    /// 【制約】NOT NULL、最大255文字
    /// </summary>
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
