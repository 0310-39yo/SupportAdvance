namespace SupportAdvance.Contexts.Authentication.Application.Queries;

/// <summary>
/// ローカル認証情報マスター Query インターフェース
///
/// 【責務】
/// - m_login_credentials テーブルからの SELECT クエリを定義
/// - ログインID でローカル認証情報を検索
///
/// 【実装】
/// - Infrastructure層で Dapper + SQL ファイルで実装
///
/// 【パターン】
/// - Read-only インターフェース（SELECT のみ）
/// - Application層は検索結果を DTO で受け取る
/// </summary>
public interface ILoginCredentialsQuery
{
    /// <summary>
    /// ログインID で認証情報を検索
    /// </summary>
    /// <param name="loginId">ログインID（従業員番号など）</param>
    /// <returns>認証情報 DTO（見つからない場合は null）</returns>
    Task<LoginCredentialsQueryResult?> GetByLoginIdAsync(string loginId);

    /// <summary>
    /// RowId で認証情報を検索
    /// </summary>
    /// <param name="rowId">m_login_credentials.row_id</param>
    /// <returns>認証情報 DTO（見つからない場合は null）</returns>
    Task<LoginCredentialsQueryResult?> GetByRowIdAsync(long rowId);
}

/// <summary>
/// ローカル認証情報マスター Query結果 DTO
///
/// 【用途】
/// - ILoginCredentialsQuery の戻り値
/// - パスワードハッシュ検証用
///
/// 【特徴】
/// - Domain Entity ではなく、Query専用DTO
/// - パスワードハッシュ（平文ではない）を含む
/// </summary>
public sealed class LoginCredentialsQueryResult
{
    /// <summary>
    /// 認証情報マスター RowId
    /// </summary>
    public long RowId { get; init; }

    /// <summary>
    /// 紐づいた従業員 RowId
    /// 【意味】社外から m_login_credentials 経由でローカル認証ログインする人物が、
    /// 社内の Employee 情報上では誰にあたるかを示すマッピング。
    /// 社外からログインした人物と社内で Employee として認識される人物は、
    /// この値を介して同一人物であることが保証される。
    /// </summary>
    public long MappingEmployeeRowId { get; init; }

    /// <summary>
    /// ログインID
    /// </summary>
    public string LoginId { get; init; } = string.Empty;

    /// <summary>
    /// パスワードハッシュ値
    /// 【注意】平文ではなく、ハッシュ化済み
    /// </summary>
    public string PasswordHash { get; init; } = string.Empty;

    /// <summary>
    /// 認証情報が有効か
    /// </summary>
    public bool IsActive { get; init; }
}
