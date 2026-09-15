using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Authentication.Domain.ValueObjects;

/// <summary>
/// 権限主体 RowId（従業員ID）
/// 【型】long（m_employees.row_id に対応）
/// 【特徴】UserAuthSession が紐づく従業員。本番環境ではこの従業員の権限が適用される
/// 【用途】current_user_row_id として t_user_auth_sessions に記録
/// 【注】Authentication BC がローカルで定義。Employee BC の EmployeeRowId と論理的に同一だが、BC境界を明確化
/// 【不変性】生成後変更不可
/// </summary>
public sealed class AuthorityRowId : RowId, IEquatable<AuthorityRowId>
{
    /// <summary>
    /// 権限主体行IDの最小有効値
    /// </summary>
    public const long MinValue = 1L;

    /// <summary>
    /// 権限主体行IDの値を取得する
    /// </summary>
    public long Value => ValueField;

    /// <summary>
    /// 指定された long 値から AuthorityRowId を生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="value">権限主体行ID（1以上）</param>
    /// <remarks>Validate は、基礎クラスのコンストラクタで自動実行される</remarks>
    private AuthorityRowId(long value) : base(value, true)
    {
    }

    /// <summary>
    /// 指定された long 値から AuthorityRowId のインスタンスを生成する
    /// </summary>
    /// <param name="value">権限主体行ID（1以上）</param>
    /// <returns>指定された行IDの AuthorityRowId のインスタンス</returns>
    /// <exception cref="ArgumentOutOfRangeException">0以下の値</exception>
    public static AuthorityRowId From(long value) => new(value);

    /// <summary>
    /// 指定された long 値から AuthorityRowId のインスタンスの生成を試みる（型安全版）
    /// </summary>
    /// <param name="value">権限主体行ID（1以上）</param>
    /// <param name="result">生成された AuthorityRowId のインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFrom(long value, out AuthorityRowId result)
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
    /// 指定された long 値から AuthorityRowId のインスタンスの生成を試みる（DB値変換版）
    /// </summary>
    /// <param name="value">DB の row_id 値</param>
    /// <param name="result">生成された AuthorityRowId のインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFromDbValue(long value, out AuthorityRowId result)
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
    /// 権限主体行IDの文字列表現を取得する
    /// </summary>
    /// <returns>数値文字列</returns>
    public override string ToString() => Value.ToString();

    /// <summary>
    /// 指定されたオブジェクトと等価かどうかを判定する
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as AuthorityRowId);

    /// <summary>
    /// 指定された AuthorityRowId と等価かどうかを判定する
    /// </summary>
    public bool Equals(AuthorityRowId? other)
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
    /// 権限主体行IDが有効か検証する
    /// </summary>
    public override void Validate(long normalized)
    {
        if (normalized < MinValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(normalized),
                normalized,
                $"AuthorityRowId must be {MinValue} or higher.");
        }
    }
}
