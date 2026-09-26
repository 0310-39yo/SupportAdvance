namespace SupportAdvance.Contexts.Authentication.Application.Services;

/// <summary>
/// パスワードハッシュ検証サービス インターフェース
/// </summary>
/// <remarks>
/// <para>【責務】</para>
/// <list type="bullet">
/// <item><description>平文パスワードと保存されたハッシュ値の検証</description></item>
/// <item><description>bcrypt による安全な検証</description></item>
/// </list>
/// <para>【実装】</para>
/// <list type="bullet">
/// <item><description>Infrastructure層で BCrypt.Net-Next ライブラリを使用</description></item>
/// </list>
/// <para>【セキュリティ】</para>
/// <list type="bullet">
/// <item><description>平文パスワードの Domain/Entity での保持なし</description></item>
/// <item><description>検証は Application層の Use Case で実施</description></item>
/// </list>
/// </remarks>
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
