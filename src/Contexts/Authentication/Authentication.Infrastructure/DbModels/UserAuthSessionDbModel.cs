using System.ComponentModel.DataAnnotations.Schema;

namespace SupportAdvance.Contexts.Identity.Infrastructure.DbModels;

/// <summary>
/// ユーザー認証セッション データベースモデル
///
/// 【責務】
///   - t_user_auth_sessions テーブルのマッピング
///   - プリミティブ型のデータベーススキーマ表現
/// 【注意】
///   - 監査フィールド（CreatedAt/UpdatedAt/DeletedAt）は DateTime プリミティブ型
///   - ログイン日時フィールド（LoggedInAt/LoggedOutAt）も DateTime
///   - LocalDateTime ↔ DateTime の変換は Mapper で実施
/// </summary>
[Table("t_user_auth_sessions")]
public class UserAuthSessionDbModel
{
    /// <summary>
    /// セッション行ID（主キー）
    /// </summary>
    [Column("row_id")]
    public long RowId { get; set; }

    /// <summary>
    /// 権限主体（従業員行ID）
    /// 【制約】NOT NULL、FK → m_employees(row_id)
    /// </summary>
    [Column("current_user_row_id")]
    public long CurrentUserRowId { get; set; }

    /// <summary>
    /// 認証方式（true=AD認証、false=ローカル認証）
    /// </summary>
    [Column("is_ad_authenticated")]
    public bool IsAdAuthenticated { get; set; }

    /// <summary>
    /// 認証成功/失敗（true=成功、false=失敗）
    /// </summary>
    [Column("login_success")]
    public bool LoginSuccess { get; set; }

    /// <summary>
    /// ログイン操作日時（成功・失敗共に記録）
    /// </summary>
    [Column("logged_in_at")]
    public DateTime LoggedInAt { get; set; }

    /// <summary>
    /// ログアウト日時（NULL=非正常終了、値あり=正常ログアウト）
    /// </summary>
    [Column("logged_out_at")]
    public DateTime? LoggedOutAt { get; set; }

    /// <summary>
    /// ローカル認証マスター行ID（ローカル認証時のみ値あり、AD認証時はNULL）
    /// 【制約】NULL許可、FK → m_login_credentials(row_id)
    /// </summary>
    [Column("login_credentials_row_id")]
    public long? LoginCredentialsRowId { get; set; }

    /// <summary>
    /// 楽観ロック用タイムスタンプ（INSERT では除外、DB が自動生成）
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
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
