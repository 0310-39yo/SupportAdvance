namespace SupportAdvance.Contexts.Authentication.Domain.ValueObjects;

/// <summary>
/// ログインID（従業員番号など）
/// </summary>
/// <remarks>
/// <para>【型】string（m_login_credentials.login_id に対応）</para>
/// <para>【特徴】ユーザーが入力するログイン用ID、ビジネス識別子</para>
/// <para>【制約】最大50文字、空文字列不可</para>
/// <para>【不変性】生成後変更不可</para>
/// </remarks>
public sealed class LoginId : IEquatable<LoginId>
{
    /// <summary>
    /// ログインID の文字列
    /// </summary>
    /// <value>前後の空白を除いた値。空文字にはならない値</value>
    public string Value { get; }

    private LoginId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("LoginId cannot be empty or whitespace", nameof(value));

        if (value.Length > 50)
            throw new ArgumentException("LoginId must be 50 characters or less", nameof(value));

        Value = value.Trim();
    }

    /// <summary>
    /// 文字列からの <see cref="LoginId"/> の生成
    /// </summary>
    /// <param name="value">ログインID。前後の空白は除去</param>
    /// <returns>生成したインスタンス</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> が空文字または空白のみの場合、または 50 文字を超える場合（長さは前後の空白を含めて判定）
    /// </exception>
    public static LoginId From(string value) => new(value);

    /// <summary>
    /// 文字列からの <see cref="LoginId"/> 生成の試行。例外の送出なし
    /// </summary>
    /// <param name="value">ログインID。外部入力のため <see langword="null"/> 許容</param>
    /// <param name="result">成功した場合は生成したインスタンス。失敗した場合は <see langword="null"/>（使用禁止）</param>
    /// <returns>
    /// 成功した場合は <see langword="true"/>。<see langword="null"/>・空文字・空白のみ・50 文字超の場合は <see langword="false"/>
    /// </returns>
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

    /// <summary>
    /// 指定した <see cref="LoginId"/> と等しいかどうかの判定
    /// </summary>
    /// <param name="other">比較対象</param>
    /// <returns><see cref="Value"/> が大文字小文字を区別して一致する場合は <see langword="true"/></returns>
    public bool Equals(LoginId? other) => other != null && Value == other.Value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as LoginId);

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// ログインID の文字列表現
    /// </summary>
    /// <returns><see cref="Value"/> そのもの</returns>
    public override string ToString() => Value;

    /// <summary>
    /// 2 つの <see cref="LoginId"/> が等しいかどうかの判定
    /// </summary>
    /// <param name="left">比較する 1 つ目の値</param>
    /// <param name="right">比較する 2 つ目の値</param>
    /// <returns>両方とも <see langword="null"/> の場合、または値が等しい場合は <see langword="true"/></returns>
    public static bool operator ==(LoginId? left, LoginId? right)
        => left?.Equals(right) ?? right is null;

    /// <summary>
    /// 2 つの <see cref="LoginId"/> が異なるかどうかの判定
    /// </summary>
    /// <param name="left">比較する 1 つ目の値</param>
    /// <param name="right">比較する 2 つ目の値</param>
    /// <returns>値が異なる場合は <see langword="true"/></returns>
    public static bool operator !=(LoginId? left, LoginId? right)
        => !(left == right);
}
