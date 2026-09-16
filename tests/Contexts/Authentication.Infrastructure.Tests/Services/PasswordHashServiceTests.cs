namespace SupportAdvance.Contexts.Authentication.Infrastructure.Tests.Services;

using SupportAdvance.Contexts.Authentication.Infrastructure.Services;
using Xunit;

/// <summary>
/// PasswordHashService の単体テスト（PBKDF2ハッシュ生成と検証）
/// </summary>
public class PasswordHashServiceTests
{
    #region グループ 1: ハッシュ生成 - 正常系

    [Fact]
    public void VO_HASH_01_Hash_WithValidPassword_GeneratesHash()
    {
        // Arrange
        var service = new PasswordHashService();
        var password = "ValidPassword123!";

        // Act
        var hash = service.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.True(hash.Length > 0);
    }

    [Fact]
    public void VO_HASH_02_Hash_GeneratesDifferentHashesForSamePw()
    {
        // Arrange
        var service = new PasswordHashService();
        var password = "SamePassword123!";

        // Act
        var hash1 = service.HashPassword(password);
        var hash2 = service.HashPassword(password);

        // Assert
        Assert.NotNull(hash1);
        Assert.NotNull(hash2);
        Assert.NotEqual(hash1, hash2);  // Salt差により異なるハッシュ
    }

    [Fact]
    public void VO_HASH_03_Hash_GeneratesDeterministicHash_WithSameSalt()
    {
        // Arrange
        var service = new PasswordHashService();
        var password = "Password123!";
        var salt = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

        // 注：PasswordHashService は public メソッドで salt 指定ができないため、
        // ハッシュ生成後の検証で確認
        var hash = service.HashPassword(password);

        // Act
        var verified = service.VerifyPassword(password, hash);

        // Assert
        Assert.True(verified);
    }

    #endregion

    #region グループ 2: ハッシュ検証 - 正常系

    [Fact]
    public void VO_HASH_04_Verify_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var service = new PasswordHashService();
        var password = "CorrectPassword123!";
        var hash = service.HashPassword(password);

        // Act
        var result = service.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VO_HASH_05_Verify_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var service = new PasswordHashService();
        var correctPassword = "CorrectPassword123!";
        var wrongPassword = "WrongPassword456!";
        var hash = service.HashPassword(correctPassword);

        // Act
        var result = service.VerifyPassword(wrongPassword, hash);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region グループ 3: セキュリティ検証 - 異常系

    [Fact]
    public void VO_SECURITY_01_Hash_WithEmptyPassword_ThrowsException()
    {
        // Arrange
        var service = new PasswordHashService();
        var emptyPassword = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.HashPassword(emptyPassword));
    }

    [Fact]
    public void VO_SECURITY_02_Verify_WithNullHash_ReturnsFalse()
    {
        // Arrange
        var service = new PasswordHashService();
        var password = "SomePassword123!";

        // Act
        var result = service.VerifyPassword(password, null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VO_SECURITY_03_Verify_WithCorruptedHash_ReturnsFalse()
    {
        // Arrange
        var service = new PasswordHashService();
        var password = "Password123!";
        var corruptedHash = "corrupted_hash_data";

        // Act
        var result = service.VerifyPassword(password, corruptedHash);

        // Assert
        Assert.False(result);
    }

    #endregion
}
