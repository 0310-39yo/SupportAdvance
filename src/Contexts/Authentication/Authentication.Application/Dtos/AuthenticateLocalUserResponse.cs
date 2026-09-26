using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Contexts.Authentication.Application.Dtos;

/// <summary>
/// ローカル認証の実行結果。ログイン成功時のセッション情報
/// </summary>
/// <remarks>
/// <para>【用途】<c>AuthenticateLocalUserUseCase</c> の戻り値</para>
/// <para>【注意】認証失敗の場合はこの型を返さず、例外を送出</para>
/// </remarks>
public sealed class AuthenticateLocalUserResponse
{
    /// <summary>
    /// 生成されたセッションの行ID
    /// </summary>
    public long UserAuthSessionRowId { get; init; }

    /// <summary>
    /// 認証済みユーザーの従業員行ID
    /// </summary>
    public long EmployeeRowId { get; init; }

    /// <summary>
    /// ユーザーのログインID
    /// </summary>
    public string LoginId { get; init; } = string.Empty;

    /// <summary>
    /// ログイン日時（JST）
    /// </summary>
    /// <value>認証セッションの記録時に <c>IClock</c> から取得した時刻</value>
    public LocalDateTime LoggedInAt { get; init; }
}
