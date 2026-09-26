namespace SupportAdvance.Contexts.Authentication.Application.Dtos;

/// <summary>
/// ローカル認証リクエスト
/// </summary>
/// <remarks>
/// <para>【責務】</para>
/// <list type="bullet">
/// <item><description>ユーザーが入力するログイン情報（ログインID、パスワード）</description></item>
/// <item><description>AuthenticateLocalUserUseCase への入力パラメータ</description></item>
/// </list>
/// <para>【特徴】</para>
/// <list type="bullet">
/// <item><description>入力値の検証は Application層で実施</description></item>
/// </list>
/// </remarks>
public sealed class AuthenticateLocalUserRequest
{
    /// <summary>
    /// ログインID（従業員番号など）
    /// </summary>
    public string LoginId { get; init; } = string.Empty;

    /// <summary>
    /// パスワード（平文）
    /// </summary>
    /// <remarks>
    /// <para>【注意】暗号化前の平文。ハッシュ検証は Use Case で実施</para>
    /// </remarks>
    public string Password { get; init; } = string.Empty;
}
