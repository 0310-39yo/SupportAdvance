namespace SupportAdvance.Contexts.Authentication.Domain.ValueObjects;

/// <summary>
/// ログインID（従業員番号など）
/// 【型】string（m_login_credentials.login_id に対応）
/// 【特徴】ユーザーが入力するログイン用ID、ビジネス識別子
/// 【制約】最大50文字、空文字列不可
/// 【不変性】生成後変更不可
/// </summary>
public sealed class LoginId : IEquatable<LoginId>
{
    public string Value { get; }

    private LoginId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("LoginId cannot be empty or whitespace", nameof(value));

        if (value.Length > 50)
            throw new ArgumentException("LoginId must be 50 characters or less", nameof(value));

        Value = value.Trim();
    }

    public static LoginId From(string value) => new(value);

    public static bool TryFrom(string? value, out LoginId result)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = null!;
                return false;
            }

            result = new LoginId(value);
            return true;
        }
        catch
        {
            result = null!;
            return false;
        }
    }

    public bool Equals(LoginId? other) => other != null && Value == other.Value;
    public override bool Equals(object? obj) => Equals(obj as LoginId);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;

    public static bool operator ==(LoginId? left, LoginId? right)
        => left?.Equals(right) ?? right is null;

    public static bool operator !=(LoginId? left, LoginId? right)
        => !(left == right);
}
