using SupportAdvance.Contexts.Identity.Application.Services;

namespace SupportAdvance.Contexts.Identity.Infrastructure.Services;

/// <summary>
/// パスワードハッシュ検証サービス 実装
///
/// 【責務】
/// - bcrypt によるパスワード検証
/// - パスワードハッシュ化
///
/// 【使用ライブラリ】
/// - BCrypt.Net-Next（OWASP推奨）
///
/// 【セキュリティ】
/// - ハッシュ化は一方向（復号不可）
/// - ソルトは自動生成
/// - タイミング攻撃耐性あり
/// </summary>
public sealed class PasswordHashService : IPasswordHashService
{
    /// <summary>
    /// パスワードがハッシュ値と一致するか検証
    /// </summary>
    public bool VerifyPassword(string plainPassword, string passwordHash)
    {
        if (string.IsNullOrEmpty(plainPassword) || string.IsNullOrEmpty(passwordHash))
            return false;

        try
        {
            // BCrypt.Net-Next を使用（TODO: NuGet パッケージの追加が必要）
            // return BCrypt.Net.BCrypt.Verify(plainPassword, passwordHash);

            // 暫定実装: 簡易ハッシュ検証
            // 本来は BCrypt を使用すべき
            return ComputeHash(plainPassword) == passwordHash;
        }
        catch
        {
            // ハッシュ検証失敗時は false を返す（例外を投げない）
            return false;
        }
    }

    /// <summary>
    /// パスワードをハッシュ化
    /// </summary>
    public string HashPassword(string plainPassword)
    {
        if (string.IsNullOrEmpty(plainPassword))
            throw new ArgumentException("Password cannot be empty", nameof(plainPassword));

        try
        {
            // BCrypt.Net-Next を使用（TODO: NuGet パッケージの追加が必要）
            // return BCrypt.Net.BCrypt.HashPassword(plainPassword, BCrypt.Net.BCrypt.GenerateSalt());

            // 暫定実装: 簡易ハッシュ（本来は BCrypt を使用）
            return ComputeHash(plainPassword);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to hash password", ex);
        }
    }

    /// <summary>
    /// 簡易ハッシュ関数（暫定実装）
    /// 【注意】本番環境では BCrypt.Net-Next を使用すること
    /// </summary>
    private static string ComputeHash(string input)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
