using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Authentication.Domain.ValueObjects;

/// <summary>
/// ログイン認証情報マスター RowId
/// 【型】long（m_login_credentials.row_id に対応）
/// 【特徴】ローカル認証時のみ参照。UserAuthSession.LoginCredentialsRowId に紐づく
/// 【不変性】生成後変更不可
/// </summary>
public sealed class LoginCredentialsRowId : RowId, IEquatable<LoginCredentialsRowId>
{
    /// <summary>
    /// ログイン認証情報マスター行IDの最小有効値
    /// </summary>
    public const long MinValue = 1L;

    /// <summary>
    /// ログイン認証情報マスター行IDの値を取得する
    /// </summary>
    public new long Value => ValueField;

    /// <summary>
    /// 指定された long 値から LoginCredentialsRowId を生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="value">ログイン認証情報マスター行ID（1以上）</param>
    /// <remarks>Validate は、基礎クラスのコンストラクタで自動実行される</remarks>
    private LoginCredentialsRowId(long value) : base(value, true)
    {
    }

    /// <summary>
    /// 指定された long 値から LoginCredentialsRowId のインスタンスを生成する
    /// </summary>
    /// <param name="value">ログイン認証情報マスター行ID（1以上）</param>
    /// <returns>指定された行IDの LoginCredentialsRowId のインスタンス</returns>
    /// <exception cref="ArgumentOutOfRangeException">0以下の値</exception>
    public static LoginCredentialsRowId From(long value) => new(value);

    /// <summary>
    /// 指定された long 値から LoginCredentialsRowId のインスタンスの生成を試みる（型安全版）
    /// </summary>
    /// <param name="value">ログイン認証情報マスター行ID（1以上）</param>
    /// <param name="result">生成された LoginCredentialsRowId のインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFrom(long value, out LoginCredentialsRowId result)
    {
        result = null!;

        try
        {
            result = From(value);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    /// <summary>
    /// 指定された long 値から LoginCredentialsRowId のインスタンスの生成を試みる（DB値変換版）
    /// </summary>
    /// <param name="value">DB の row_id 値</param>
    /// <param name="result">生成された LoginCredentialsRowId のインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFromDbValue(long value, out LoginCredentialsRowId result)
    {
        result = null!;

        try
        {
            result = From(value);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    /// <summary>
    /// ログイン認証情報マスター行IDの文字列表現を取得する
    /// </summary>
    /// <returns>数値文字列</returns>
    public override string ToString() => Value.ToString();

    /// <summary>
    /// 指定されたオブジェクトと等価かどうかを判定する
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as LoginCredentialsRowId);

    /// <summary>
    /// 指定された LoginCredentialsRowId と等価かどうかを判定する
    /// </summary>
    public bool Equals(LoginCredentialsRowId? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Value == other.Value;
    }

    /// <summary>
    /// ハッシュコードを取得する
    /// </summary>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// ログイン認証情報マスター行IDが有効か検証する
    /// </summary>
    public override void Validate(long normalized)
    {
        if (normalized < MinValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(normalized),
                normalized,
                $"LoginCredentialsRowId must be {MinValue} or higher.");
        }
    }
}
