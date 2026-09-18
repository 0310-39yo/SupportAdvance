using System.Security.Cryptography;
using SupportAdvance.Contexts.Authentication.Application.Services;

namespace SupportAdvance.Contexts.Authentication.Infrastructure.Services;

/// <summary>
/// パスワードハッシュ検証サービス 実装
///
/// 【責務】
/// - PBKDF2 (with Salt) によるパスワード検証
/// - パスワードハッシュ化
///
/// 【ハッシュ方式】
/// - PBKDF2-SHA256（イテレーション回数：10000）
/// - Salt: 16バイト（ランダム生成）
/// - Hash: 32バイト
/// - ハッシュ形式: Base64(Salt + Hash) = 64文字
///
/// 【セキュリティ】
/// - ハッシュ化は一方向（復号不可）
/// - ソルトはランダム生成
/// - タイミング攻撃耐性あり
/// </summary>
public sealed class PasswordHashService : IPasswordHashService
{
    private const int SaltSize = 16;      // バイト
    private const int HashSize = 32;      // バイト
    private const int Iterations = 10000; // PBKDF2 イテレーション

    /// <summary>
    /// パスワードがハッシュ値と一致するか検証（PBKDF2+Salt）
    ///
    /// 【アルゴリズム】
    /// 1. DB ハッシュから Salt を抽出（最初の 16 バイト）
    /// 2. 入力パスワードを同じ Salt で PBKDF2 ハッシュ化
    /// 3. 生成されたハッシュ部分と DB のハッシュ部分を比較
    /// </summary>
    /// <param name="plainPassword">入力されたパスワード（平文）</param>
    /// <param name="passwordHash"><see cref="HashPassword"/> で生成した Base64 文字列（Salt 16 バイト + ハッシュ 32 バイト）</param>
    /// <returns>一致した場合は <see langword="true"/>。不一致、いずれかが空、またはハッシュの形式が不正な場合は <see langword="false"/>（例外の送出なし）</returns>
    public bool VerifyPassword(string plainPassword, string passwordHash)
    {
        if (string.IsNullOrEmpty(plainPassword) || string.IsNullOrEmpty(passwordHash))
            return false;

        try
        {
            // Base64 デコード
            byte[] saltAndHash;
            try
            {
                saltAndHash = Convert.FromBase64String(passwordHash);
            }
            catch
            {
                // Base64 デコード失敗 = 無効なハッシュ形式
                return false;
            }

            // ハッシュが最低限の長さか確認（Salt + Hash）
            if (saltAndHash.Length < SaltSize + HashSize)
                return false;

            // Salt を抽出（最初の 16 バイト）
            byte[] salt = new byte[SaltSize];
            Array.Copy(saltAndHash, 0, salt, 0, SaltSize);

            // DB に格納されたハッシュを抽出（後ろ 32 バイト）
            byte[] storedHash = new byte[HashSize];
            Array.Copy(saltAndHash, SaltSize, storedHash, 0, HashSize);

            // 入力パスワードを同じ Salt で PBKDF2 ハッシュ化
            byte[] computedHash = Rfc2898DeriveBytes.Pbkdf2(plainPassword, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

            // ハッシュ部分を比較（タイミング攻撃対策）
            return ConstantTimeComparison(storedHash, computedHash);
        }
        catch
        {
            // ハッシュ検証失敗時は false を返す（例外を投げない）
            return false;
        }
    }

    /// <summary>
    /// パスワードをハッシュ化（PBKDF2+Salt）
    /// </summary>
    /// <param name="plainPassword">ハッシュ化するパスワード（平文）</param>
    /// <returns>ランダムな Salt（16 バイト）と PBKDF2-SHA256 のハッシュ（32 バイト）を連結した Base64 文字列。同じパスワードでも毎回異なる値</returns>
    /// <exception cref="ArgumentException"><paramref name="plainPassword"/> が <see langword="null"/> または空文字の場合</exception>
    /// <exception cref="InvalidOperationException">ハッシュ化の処理に失敗した場合</exception>
    public string HashPassword(string plainPassword)
    {
        if (string.IsNullOrEmpty(plainPassword))
            throw new ArgumentException("Password cannot be empty", nameof(plainPassword));

        try
        {
            // Salt を生成（16バイト）
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // PBKDF2 でハッシュ化（イテレーション回数：10000）
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(plainPassword, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

            // Salt + Hash を結合して Base64 エンコード
            byte[] saltAndHash = new byte[salt.Length + hash.Length];
            Buffer.BlockCopy(salt, 0, saltAndHash, 0, salt.Length);
            Buffer.BlockCopy(hash, 0, saltAndHash, salt.Length, hash.Length);

            return Convert.ToBase64String(saltAndHash);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to hash password", ex);
        }
    }

    /// <summary>
    /// タイミング攻撃対策：2つのバイト配列を時間固定で比較
    /// </summary>
    private static bool ConstantTimeComparison(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;

        int result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];  // XOR で差分を蓄積（全バイト比較）
        }

        return result == 0;
    }
}
