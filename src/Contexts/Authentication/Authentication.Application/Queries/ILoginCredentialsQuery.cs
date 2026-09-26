namespace SupportAdvance.Contexts.Authentication.Application.Queries;

/// <summary>
/// ローカル認証情報マスター Query インターフェース
/// </summary>
/// <remarks>
/// <para>【責務】</para>
/// <list type="bullet">
/// <item><description>m_login_credentials テーブルからの SELECT クエリを定義</description></item>
/// <item><description>ログインID でローカル認証情報を検索</description></item>
/// </list>
/// <para>【実装】</para>
/// <list type="bullet">
/// <item><description>Infrastructure層で Dapper + SQL ファイルで実装</description></item>
/// </list>
/// <para>【パターン】</para>
/// <list type="bullet">
/// <item><description>Read-only インターフェース（SELECT のみ）</description></item>
/// <item><description>Application層は検索結果を DTO で受け取る</description></item>
/// </list>
/// </remarks>
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
/// </summary>
/// <remarks>
/// <para>【用途】</para>
/// <list type="bullet">
/// <item><description>ILoginCredentialsQuery の戻り値</description></item>
/// <item><description>パスワードハッシュ検証用</description></item>
/// </list>
/// <para>【特徴】</para>
/// <list type="bullet">
/// <item><description>Domain Entity ではなく、Query専用DTO</description></item>
/// <item><description>パスワードハッシュ（平文ではない）を含む</description></item>
/// </list>
/// </remarks>
public sealed class LoginCredentialsQueryResult
{
    /// <summary>
    /// 認証情報マスター RowId
    /// </summary>
    public long RowId { get; init; }

    /// <summary>
    /// 紐づいた従業員 RowId
    /// </summary>
    /// <remarks>
    /// <para>【意味】社外から m_login_credentials 経由でローカル認証ログインする人物が、社内の Employee 情報上では誰にあたるかを示すマッピング。社外からログインした人物と社内で Employee として認識される人物は、この値を介して同一人物であることの保証</para>
    /// </remarks>
    public long MappingEmployeeRowId { get; init; }

    /// <summary>
    /// ログインID
    /// </summary>
    public string LoginId { get; init; } = string.Empty;

    /// <summary>
    /// パスワードハッシュ値
    /// </summary>
    /// <remarks>
    /// <para>【注意】平文ではなく、ハッシュ化済み</para>
    /// </remarks>
    public string PasswordHash { get; init; } = string.Empty;

    /// <summary>
    /// 認証情報が有効か
    /// </summary>
    public bool IsActive { get; init; }
}
