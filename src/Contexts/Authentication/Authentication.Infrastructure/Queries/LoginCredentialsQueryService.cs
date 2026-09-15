using Dapper;
using SupportAdvance.Contexts.Authentication.Application.Queries;
using SupportAdvance.Infrastructure.Persistence;

namespace SupportAdvance.Contexts.Authentication.Infrastructure.Queries;

/// <summary>
/// ローカル認証情報マスター Query Service 実装
///
/// 【責務】
/// - m_login_credentials テーブルから SELECT
/// - ILoginCredentialsQuery の実装
/// - Dapper + SQL ファイルで実装
/// </summary>
public sealed class LoginCredentialsQueryService : ILoginCredentialsQuery
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly SqlQueryLoader _sqlQueryLoader;

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
