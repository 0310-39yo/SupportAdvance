namespace SupportAdvance.Contexts.Identity.Application.Dtos;

/// <summary>
/// ローカル認証レスポンス
///
/// 【責務】
/// - AuthenticateLocalUserUseCase の実行結果
/// - ログイン成功時のセッション情報
///
/// 【特徴】
/// - 成功時のみ値を保持
/// - 認証失敗時は例外をスロー（レスポンスではなく例外処理）
/// </summary>
public sealed class AuthenticateLocalUserResponse
{
    /// <summary>
    /// 生成されたセッション RowId
    /// </summary>
    public long UserAuthSessionRowId { get; init; }

    /// <summary>
    /// 認証済みユーザーの従業員 RowId
    /// </summary>
    public long EmployeeRowId { get; init; }

    /// <summary>
    /// ユーザーのログインID
    /// </summary>
    public string LoginId { get; init; } = string.Empty;

    /// <summary>
    /// ログイン日時（ISO 8601 形式）
    /// </summary>
    public DateTime LoggedInAt { get; init; }
}
