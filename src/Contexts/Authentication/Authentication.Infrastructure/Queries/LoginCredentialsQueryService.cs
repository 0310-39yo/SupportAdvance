using Dapper;
using SupportAdvance.Contexts.Authentication.Application.Queries;
using SupportAdvance.Infrastructure.Persistence;

namespace SupportAdvance.Contexts.Authentication.Infrastructure.Queries;

/// <summary>
/// ローカル認証情報マスター Query Service 実装
/// </summary>
/// <remarks>
/// <para>【責務】</para>
/// <list type="bullet">
/// <item><description>m_login_credentials テーブルから SELECT</description></item>
/// <item><description>ILoginCredentialsQuery の実装</description></item>
/// <item><description>Dapper + SQL ファイルで実装</description></item>
/// </list>
/// </remarks>
public sealed class LoginCredentialsQueryService : ILoginCredentialsQuery
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly SqlQueryLoader _sqlQueryLoader;

    /// <summary>
    /// <see cref="LoginCredentialsQueryService"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="connectionFactory">DB 接続の生成元</param>
    /// <param name="sqlQueryLoader">検索用 SQL ファイルの読み込み元</param>
    /// <exception cref="ArgumentNullException">いずれかの引数が <see langword="null"/> の場合</exception>
    public LoginCredentialsQueryService(
        IDbConnectionFactory connectionFactory,
        SqlQueryLoader sqlQueryLoader)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _sqlQueryLoader = sqlQueryLoader ?? throw new ArgumentNullException(nameof(sqlQueryLoader));
    }

    /// <summary>
    /// ログインID で認証情報を検索
    /// </summary>
    /// <param name="loginId">検索するログインID</param>
    /// <returns>見つかった認証情報。<paramref name="loginId"/> が空・空白のみの場合、または見つからない場合は <see langword="null"/></returns>
    public async Task<LoginCredentialsQueryResult?> GetByLoginIdAsync(string loginId)
    {
        if (string.IsNullOrWhiteSpace(loginId))
            return null;

        var sql = _sqlQueryLoader.LoadQuery("LoginCredentials.GetLoginCredentialsByLoginId", typeof(LoginCredentialsQueryService));

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<LoginCredentialsQueryResult>(
            sql,
            new { LoginId = loginId });

        return result;
    }

    /// <summary>
    /// RowId で認証情報を検索
    /// </summary>
    /// <param name="rowId">検索する認証情報の行ID</param>
    /// <returns>見つかった認証情報。<paramref name="rowId"/> が 0 以下の場合、または見つからない（論理削除済みを含む）場合は <see langword="null"/></returns>
    public async Task<LoginCredentialsQueryResult?> GetByRowIdAsync(long rowId)
    {
        if (rowId <= 0)
            return null;

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<LoginCredentialsQueryResult>(
            @"
            SELECT
                [row_id],
                [mapping_employee_row_id],
                [login_id],
                [password_hash],
                [is_active]
            FROM
                [m_login_credentials]
            WHERE
                [row_id] = @RowId
                AND [deleted_at] IS NULL
            ",
            new { RowId = rowId });

        return result;
    }
}
