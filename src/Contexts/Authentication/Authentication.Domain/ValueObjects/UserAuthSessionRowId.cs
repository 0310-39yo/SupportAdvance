using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Authentication.Domain.ValueObjects;

/// <summary>
/// ユーザー認証セッションレコード RowId（主キー）
/// 【型】long（t_user_auth_sessions.row_id に対応）
/// 【特徴】UserAuthSession 集約の一意識別子、Sequence自動採番
/// 【不変性】生成後変更不可
/// </summary>
public sealed class UserAuthSessionRowId : RowId, IEquatable<UserAuthSessionRowId>
{
    /// <summary>
    /// ユーザー認証セッションレコード行IDの最小有効値
    /// </summary>
    public const long MinValue = 1L;

    /// <summary>
    /// ユーザー認証セッションレコード行IDの値を取得する
    /// </summary>
    public new long Value => ValueField;

    /// <summary>
    /// 指定された long 値から UserAuthSessionRowId を生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="value">ユーザー認証セッションレコード行ID（1以上）</param>
    /// <remarks>Validate は、基礎クラスのコンストラクタで自動実行される</remarks>
    private UserAuthSessionRowId(long value) : base(value, true)
    {
    }

    /// <summary>
    /// 指定された long 値から UserAuthSessionRowId のインスタンスを生成する
    /// </summary>
    /// <param name="value">ユーザー認証セッションレコード行ID（1以上）</param>
    /// <returns>指定された行IDの UserAuthSessionRowId のインスタンス</returns>
    /// <exception cref="ArgumentOutOfRangeException">0以下の値</exception>
    public static UserAuthSessionRowId From(long value) => new(value);

    /// <summary>
    /// 指定された long 値から UserAuthSessionRowId のインスタンスの生成を試みる（型安全版）
    /// </summary>
    /// <param name="value">ユーザー認証セッションレコード行ID（1以上）</param>
    /// <param name="result">生成された UserAuthSessionRowId のインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFrom(long value, out UserAuthSessionRowId result)
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
    /// 指定された long 値から UserAuthSessionRowId のインスタンスの生成を試みる（DB値変換版）
    /// </summary>
    /// <param name="value">DB の row_id 値</param>
    /// <param name="result">生成された UserAuthSessionRowId のインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFromDbValue(long value, out UserAuthSessionRowId result)
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
    /// ユーザー認証セッションレコード行IDの文字列表現を取得する
    /// </summary>
    /// <returns>数値文字列</returns>
    public override string ToString() => Value.ToString();

    /// <summary>
    /// 指定されたオブジェクトと等価かどうかを判定する
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as UserAuthSessionRowId);

    /// <summary>
    /// 指定された UserAuthSessionRowId と等価かどうかを判定する
    /// </summary>
    public bool Equals(UserAuthSessionRowId? other)
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
    /// ユーザー認証セッションレコード行IDが有効か検証する
    /// </summary>
    public override void Validate(long normalized)
    {
        if (normalized < MinValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(normalized),
                normalized,
                $"UserAuthSessionRowId must be {MinValue} or higher.");
        }
    }
}
