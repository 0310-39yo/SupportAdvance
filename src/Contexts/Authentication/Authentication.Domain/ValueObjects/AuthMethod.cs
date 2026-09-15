namespace SupportAdvance.Contexts.Identity.Domain.ValueObjects;

/// <summary>
/// 認証方式（LocalAuth / WindowsAD）
/// 【型】enum（論理値：ビジネスロジックで判別）
/// 【値】LocalAuth = ローカルユーザー認証（m_login_credentials）
///     WindowsAD = Windows Active Directory 認証
/// 【特徴】ログイン時の認証方式を判別（5段階検証ロジックで使用）
/// 【不変性】生成後変更不可
/// </summary>
public sealed class AuthMethod : IEquatable<AuthMethod>
{
    public enum MethodType
    {
        /// <summary>ローカルユーザー認証（ID/パスワード）</summary>
        LocalAuth = 0,

        /// <summary>Windows Active Directory 認証</summary>
        WindowsAD = 1
    }

    public MethodType Value { get; }

    private AuthMethod(MethodType value)
    {
        if (!Enum.IsDefined(typeof(MethodType), value))
            throw new ArgumentException($"Invalid AuthMethod: {value}", nameof(value));

        Value = value;
    }

    public static AuthMethod From(MethodType value) => new(value);

    public static AuthMethod LocalAuth() => new(MethodType.LocalAuth);
    public static AuthMethod WindowsAD() => new(MethodType.WindowsAD);

    public static bool TryFrom(int value, out AuthMethod result)
    {
        try
        {
            if (!Enum.IsDefined(typeof(MethodType), value))
            {
                result = null!;
                return false;
            }

            result = new AuthMethod((MethodType)value);
            return true;
        }
        catch
        {
            result = null!;
            return false;
        }
    }

    public bool IsLocalAuth() => Value == MethodType.LocalAuth;
    public bool IsWindowsAD() => Value == MethodType.WindowsAD;

    public bool Equals(AuthMethod? other) => other != null && Value == other.Value;
    public override bool Equals(object? obj) => Equals(obj as AuthMethod);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();

    public static bool operator ==(AuthMethod? left, AuthMethod? right)
        => left?.Equals(right) ?? right is null;

    public static bool operator !=(AuthMethod? left, AuthMethod? right)
        => !(left == right);
}
