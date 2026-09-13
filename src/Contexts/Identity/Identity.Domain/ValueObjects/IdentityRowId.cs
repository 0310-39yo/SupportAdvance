using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Identity.Domain.ValueObjects;

/// <summary>
/// 認証レコードRowId（主キー）
/// 【型】long（m_user_auth_sessions.row_id に対応）
/// 【特徴】Identity 集約の一意識別子、Sequence自動採番
/// 【不変性】生成後変更不可
/// </summary>
public sealed class IdentityRowId : RowId, IEquatable<IdentityRowId>
{
    /// <summary>
    /// 認証レコード行IDの最小有効値
    /// </summary>
    public const long MinValue = 1L;

    /// <summary>
    /// 認証レコード行IDの値を取得する
    /// </summary>
    public long Value => ValueField;

    /// <summary>
    /// 指定された long 値からIdentityRowIdを生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="value">認証レコード行ID（1以上）</param>
    /// <remarks>Validate は、基礎クラスのコンストラクタで自動実行される</remarks>
    private IdentityRowId(long value) : base(value, true)
    {
    }

    /// <summary>
    /// 指定された long 値からIdentityRowIdのインスタンスを生成する
    /// </summary>
    /// <param name="value">認証レコード行ID（1以上）</param>
    /// <returns>指定された行IDのIdentityRowIdのインスタンス</returns>
    /// <exception cref="ArgumentOutOfRangeException">0以下の値</exception>
    public static IdentityRowId From(long value) => new(value);

    /// <summary>
    /// 指定された long 値からIdentityRowIdのインスタンスの生成を試みる（型安全版）
    /// </summary>
    /// <param name="value">認証レコード行ID（1以上）</param>
    /// <param name="result">生成されたIdentityRowIdのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFrom(long value, out IdentityRowId result)
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
    /// 指定された long 値からIdentityRowIdのインスタンスの生成を試みる（DB値変換版）
    /// </summary>
    /// <param name="value">DB の row_id 値</param>
    /// <param name="result">生成されたIdentityRowIdのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFromDbValue(long value, out IdentityRowId result)
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
    /// 認証レコード行IDの文字列表現を取得する
    /// </summary>
    /// <returns>数値文字列</returns>
    public override string ToString() => Value.ToString();

    /// <summary>
    /// 指定されたオブジェクトと等価かどうかを判定する
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as IdentityRowId);

    /// <summary>
    /// 指定されたIdentityRowIdと等価かどうかを判定する
    /// </summary>
    public bool Equals(IdentityRowId? other)
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
    /// 認証レコード行IDが有効か検証する
    /// </summary>
    public override void Validate(long normalized)
    {
        if (normalized < MinValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(normalized),
                normalized,
                $"IdentityRowId must be {MinValue} or higher.");
        }
    }
}
