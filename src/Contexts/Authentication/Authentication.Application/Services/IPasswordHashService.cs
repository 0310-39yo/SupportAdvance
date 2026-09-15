namespace SupportAdvance.Contexts.Authentication.Application.Services;

/// <summary>
/// パスワードハッシュ検証サービス インターフェース
///
/// 【責務】
/// - 平文パスワードと保存されたハッシュ値の検証
/// - bcrypt による安全な検証
///
/// 【実装】
/// - Infrastructure層で BCrypt.Net-Next ライブラリを使用
///
/// 【セキュリティ】
/// - 平文パスワードを Domain/Entity で保持しない
/// - 検証は Application層の Use Case で実施
/// </summary>
public interface IPasswordHashService
{
    /// <summary>
    /// パスワードがハッシュ値と一致するか検証
    /// </summary>
    /// <param name="plainPassword">入力されたパスワード（平文）</param>
    /// <param name="passwordHash">DB に保存されたハッシュ値</param>
    /// <returns>一致する場合は true、一致しない場合は false</returns>
    bool VerifyPassword(string plainPassword, string passwordHash);

    /// <summary>
    /// パスワードをハッシュ化
    /// </summary>
    /// <param name="plainPassword">平文パスワード</param>
    /// <returns>ハッシュ化されたパスワード</returns>
    string HashPassword(string plainPassword);
}
