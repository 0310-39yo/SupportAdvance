namespace SupportAdvance.Contexts.Identity.Application.Dtos;

/// <summary>
/// ローカル認証リクエスト
///
/// 【責務】
/// - ユーザーが入力するログイン情報（ログインID、パスワード）
/// - AuthenticateLocalUserUseCase への入力パラメータ
///
/// 【特徴】
/// - 入力値の検証は Application層で実施
/// </summary>
public sealed class AuthenticateLocalUserRequest
{
    /// <summary>
    /// ログインID（従業員番号など）
    /// </summary>
    public string LoginId { get; init; } = string.Empty;

    /// <summary>
    /// パスワード（平文）
    /// 【注意】暗号化前の平文。ハッシュ検証は Use Case で実施
    /// </summary>
    public string Password { get; init; } = string.Empty;
}
