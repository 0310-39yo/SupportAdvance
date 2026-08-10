namespace SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.DataAccess.Models;

/// <summary>
/// t_UserPreferences のDB永続化モデル
///
/// 【責務】Domain Entity をDB用にマッピング
/// 【用途】RepoDb/Dapper での行データ表現
/// 【テーブル種別】トランザクションテーブル（t_*）
///
/// 【監査カラム】
/// - row_id: 主キー（Sequence自動採番）
/// - row_version: 楽観ロック用タイムスタンプ
/// - created_at/created_by: 作成者・日時
/// - updated_at/updated_by: 更新者・日時
/// - deleted_at/deleted_by: 削除者・日時（論理削除）
/// 【型】DateTime プリミティブ型のみ（Infrastructure層の責務）
/// 【注意】Domain層の LocalDateTime は Mapper で DateTime に変換
/// </summary>
public class UserPreferencesDbModel
{
    /// <summary>
    /// 主キー（Sequence自動採番）
    /// </summary>
    public long RowId { get; set; }

    /// <summary>
    /// 集約のビジネスID（UserPreferencesId、GUID）
    /// 【用途】Domain Entity.Id の識別子
    /// </summary>
    public Guid UserPreferencesId { get; set; }

    /// <summary>
    /// ユーザーID（回答者ユーザーID、1000～9999）
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 楽観ロック用タイムスタンプ
    /// </summary>
    public byte[] RowVersion { get; set; } = null!;

    /// <summary>
    /// 作成日時（DateTime, JST、ORM マッピング用プリミティブ型）
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 作成者（m_persons.row_id）
    /// </summary>
    public long CreatedBy { get; set; }

    /// <summary>
    /// 更新日時（DateTime?, JST、ORM マッピング用プリミティブ型）
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 更新者（m_persons.row_id）
    /// </summary>
    public long? UpdatedBy { get; set; }

    /// <summary>
    /// 削除日時（DateTime?, JST、ORM マッピング用プリミティブ型）
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 削除者（m_persons.row_id）
    /// </summary>
    public long? DeletedBy { get; set; }

    /// <summary>
    /// 希望車種（CarModel内部値）
    /// </summary>
    public int? PreferredModel { get; set; }

    /// <summary>
    /// 希望ボディタイプ
    /// </summary>
    public string? PreferredBodyType { get; set; }

    /// <summary>
    /// オートマ希望フラグ
    /// </summary>
    public bool PrefersAutomatic { get; set; }

    /// <summary>
    /// 予算下限（円）
    /// </summary>
    public decimal? BudgetFrom { get; set; }

    /// <summary>
    /// 予算上限（円）
    /// </summary>
    public decimal? BudgetTo { get; set; }
}
