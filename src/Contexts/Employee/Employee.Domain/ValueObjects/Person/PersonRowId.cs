using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Person;

/// <summary>
/// データベース上の人物レコードの行ID（rowId）を表すValueObject
/// 【範囲】1以上（long.MaxValue以下）
/// 【責務】m_persons.row_id の管理と検証
/// </summary>
public sealed class PersonRowId : RowId, IEquatable<PersonRowId>
{
    /// <summary>
    /// 人物行IDの最小有効値
    /// </summary>
    public const long MinValue = 1L;

    /// <summary>
    /// 人物行IDの値を取得する
    /// </summary>
    public long Value => ValueField;

    /// <summary>
    /// 指定された long 値からPersonRowIdを生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="value">人物行ID（1以上）</param>
    /// <remarks>Validate は、基礎クラスのコンストラクタで自動実行される</remarks>
    private PersonRowId(long value) : base(value, true)
    {
    }

    /// <summary>
    /// 指定された long 値からPersonRowIdのインスタンスを生成する（推奨: Domain層での生成方式）
    /// 【責務】long値から人物行IDを表現する
    /// </summary>
    /// <param name="value">人物行ID（1以上）</param>
    /// <returns>指定された行IDのPersonRowIdのインスタンス</returns>
    /// <exception cref="ArgumentOutOfRangeException">0以下の値</exception>
    public static PersonRowId From(long value) => new(value);

    /// <summary>
    /// 指定された long 値からPersonRowIdのインスタンスの生成を試みる（型安全版）
    /// 【責務】null安全に PersonRowId を生成する（Domain層での生成方式）
    /// </summary>
    /// <param name="value">人物行ID</param>
    /// <param name="result">生成されたPersonRowIdのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFrom(long value, out PersonRowId result)
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
    /// 指定された long 値からPersonRowIdのインスタンスの生成を試みる（DB値変換版）
    /// 【責務】DB から読み込んだ long から PersonRowId を復元
    /// </summary>
    /// <param name="value">DB の person.row_id 値</param>
    /// <param name="result">生成されたPersonRowIdのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFromDbValue(long value, out PersonRowId result)
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
    /// 人物行IDの文字列表現を取得する
    /// 【責務】数値を文字列に変換
    /// </summary>
    /// <returns>数値文字列（例："12345"）</returns>
    public override string ToString() => Value.ToString();

    /// <summary>
    /// 指定されたオブジェクトと等価かどうかを判定する
    /// </summary>
    /// <param name="obj">比較対象のオブジェクト</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public override bool Equals(object? obj) => Equals(obj as PersonRowId);

    /// <summary>
    /// 指定されたPersonRowIdと等価かどうかを判定する
    /// 【責務】指定されたPersonRowIdと等価かどうかを判定する
    /// </summary>
    /// <param name="other">比較対象のPersonRowId</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public bool Equals(PersonRowId? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Value == other.Value;
    }

    /// <summary>
    /// ハッシュコードを取得する
    /// 【責務】オブジェクトのハッシュコードを取得する
    /// </summary>
    /// <returns>オブジェクトのハッシュコード</returns>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// 人物行IDが有効か検証する
    /// </summary>
    /// <param name="normalized">検証対象の値</param>
    /// <exception cref="ArgumentOutOfRangeException">0以下の値</exception>
    public override void Validate(long normalized)
    {
        // 1以上か確認
        if (normalized < MinValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(normalized),
                normalized,
                $"PersonRowId must be {MinValue} or higher.");
        }
    }
}
