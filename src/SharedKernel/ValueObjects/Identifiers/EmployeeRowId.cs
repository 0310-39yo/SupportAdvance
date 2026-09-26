namespace SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// データベース上の従業員レコードの行ID（rowId）を表すValueObject
/// </summary>
/// <remarks>
/// <para>【範囲】1以上（long.MaxValue以下）</para>
/// <para>【責務】t_employees.row_id の管理と検証</para>
/// </remarks>
public sealed class EmployeeRowId : RowId, IEquatable<EmployeeRowId>
{
    /// <summary>
    /// 従業員行IDの最小有効値
    /// </summary>
    public const long MinValue = 1L;

    /// <summary>
    /// 従業員行IDの値の取得
    /// </summary>
    public new long Value => ValueField;

    /// <summary>
    /// 指定された long 値からEmployeeRowIdを生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="value">従業員行ID（1以上）</param>
    /// <remarks>Validate は、基礎クラスのコンストラクタでの自動実行</remarks>
    private EmployeeRowId(long value) : base(value, true)
    {
    }

    /// <summary>
    /// 指定された long 値からEmployeeRowIdのインスタンスを生成する（推奨: Domain層での生成方式）
    /// </summary>
    /// <param name="value">従業員行ID（1以上）</param>
    /// <returns>指定された行IDのEmployeeRowIdのインスタンス</returns>
    /// <exception cref="ArgumentOutOfRangeException">0以下の値</exception>
    /// <remarks>
    /// <para>【責務】long値から従業員行IDの表現</para>
    /// </remarks>
    public static EmployeeRowId From(long value) => new(value);

    /// <summary>
    /// 指定された long 値からEmployeeRowIdのインスタンスの生成を試みる（型安全版）
    /// </summary>
    /// <param name="value">従業員行ID</param>
    /// <param name="result">生成されたEmployeeRowIdのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    /// <remarks>
    /// <para>【責務】null安全に EmployeeRowId を生成する（Domain層での生成方式）</para>
    /// </remarks>
    public static bool TryFrom(long value, out EmployeeRowId result)
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
    /// 指定された long 値からEmployeeRowIdのインスタンスの生成を試みる（DB値変換版）
    /// </summary>
    /// <param name="value">DB の employee.row_id 値</param>
    /// <param name="result">生成されたEmployeeRowIdのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    /// <remarks>
    /// <para>【責務】DB から読み込んだ long から EmployeeRowId を復元</para>
    /// </remarks>
    public static bool TryFromDbValue(long value, out EmployeeRowId result)
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
    /// 従業員行IDの文字列表現の取得
    /// </summary>
    /// <returns>数値文字列（例："12345"）</returns>
    /// <remarks>
    /// <para>【責務】数値を文字列に変換</para>
    /// </remarks>
    public override string ToString() => Value.ToString();

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as EmployeeRowId);

    /// <summary>
    /// 指定されたEmployeeRowIdと等価かどうかの判定
    /// </summary>
    /// <param name="other">比較対象のEmployeeRowId</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    /// <remarks>
    /// <para>【責務】指定されたEmployeeRowIdと等価かどうかの判定</para>
    /// </remarks>
    public bool Equals(EmployeeRowId? other)
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

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// 従業員行IDが有効か検証
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
                $"EmployeeRowId must be {MinValue} or higher.");
        }
    }
}
