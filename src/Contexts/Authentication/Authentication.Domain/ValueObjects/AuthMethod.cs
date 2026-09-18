namespace SupportAdvance.Contexts.Authentication.Domain.ValueObjects;

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
    /// <summary>
    /// 認証方式の区分
    /// </summary>
    /// <remarks>
    /// <para>【重要】数値は <see cref="TryFrom"/> で整数から復元されるため、既存の値の変更・並べ替えは禁止</para>
    /// </remarks>
    public enum MethodType
    {
        /// <summary>
        /// ローカルユーザー認証（ID/パスワード）
        /// </summary>
        LocalAuth = 0,

        /// <summary>
        /// Windows Active Directory 認証
        /// </summary>
        WindowsAD = 1
    }

    /// <summary>
    /// 認証方式の区分値
    /// </summary>
    /// <value><see cref="MethodType"/> に定義済みの値のみ</value>
    public MethodType Value { get; }

    private AuthMethod(MethodType value)
    {
        if (!Enum.IsDefined(typeof(MethodType), value))
            throw new ArgumentException($"Invalid AuthMethod: {value}", nameof(value));

        Value = value;
    }

    /// <summary>
    /// 区分値からの <see cref="AuthMethod"/> の生成
    /// </summary>
    /// <param name="value">認証方式の区分値</param>
    /// <returns>生成したインスタンス</returns>
    /// <exception cref="ArgumentException"><paramref name="value"/> が <see cref="MethodType"/> に未定義の値の場合</exception>
    public static AuthMethod From(MethodType value) => new(value);

    /// <summary>
    /// ローカル認証を表す <see cref="AuthMethod"/> の生成
    /// </summary>
    /// <returns><see cref="MethodType.LocalAuth"/> のインスタンス</returns>
    public static AuthMethod LocalAuth() => new(MethodType.LocalAuth);

    /// <summary>
    /// Windows AD 認証を表す <see cref="AuthMethod"/> の生成
    /// </summary>
    /// <returns><see cref="MethodType.WindowsAD"/> のインスタンス</returns>
    public static AuthMethod WindowsAD() => new(MethodType.WindowsAD);

    /// <summary>
    /// 整数値（DB 値など）からの <see cref="AuthMethod"/> 生成の試行。例外の送出なし
    /// </summary>
    /// <param name="value"><see cref="MethodType"/> の数値</param>
    /// <param name="result">成功した場合は生成したインスタンス。失敗した場合は <see langword="null"/>（使用禁止）</param>
    /// <returns>成功した場合は <see langword="true"/>。<see cref="MethodType"/> に未定義の値の場合は <see langword="false"/></returns>
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

    /// <summary>
    /// ローカル認証かどうかの判定
    /// </summary>
    /// <returns><see cref="MethodType.LocalAuth"/> の場合は <see langword="true"/></returns>
    public bool IsLocalAuth() => Value == MethodType.LocalAuth;

    /// <summary>
    /// Windows AD 認証かどうかの判定
    /// </summary>
    /// <returns><see cref="MethodType.WindowsAD"/> の場合は <see langword="true"/></returns>
    public bool IsWindowsAD() => Value == MethodType.WindowsAD;

    /// <summary>
    /// 指定した <see cref="AuthMethod"/> と等しいかどうかの判定
    /// </summary>
    /// <param name="other">比較対象</param>
    /// <returns><see cref="Value"/> が等しい場合は <see langword="true"/></returns>
    public bool Equals(AuthMethod? other) => other != null && Value == other.Value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as AuthMethod);

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// 認証方式の文字列表現
    /// </summary>
    /// <returns>区分名（<c>LocalAuth</c> または <c>WindowsAD</c>）</returns>
    public override string ToString() => Value.ToString();

    /// <summary>
    /// 2 つの <see cref="AuthMethod"/> が等しいかどうかの判定
    /// </summary>
    /// <param name="left">比較する 1 つ目の値</param>
    /// <param name="right">比較する 2 つ目の値</param>
    /// <returns>両方とも <see langword="null"/> の場合、または値が等しい場合は <see langword="true"/></returns>
    public static bool operator ==(AuthMethod? left, AuthMethod? right)
        => left?.Equals(right) ?? right is null;

    /// <summary>
    /// 2 つの <see cref="AuthMethod"/> が異なるかどうかの判定
    /// </summary>
    /// <param name="left">比較する 1 つ目の値</param>
    /// <param name="right">比較する 2 つ目の値</param>
    /// <returns>値が異なる場合は <see langword="true"/></returns>
    public static bool operator !=(AuthMethod? left, AuthMethod? right)
        => !(left == right);
}
