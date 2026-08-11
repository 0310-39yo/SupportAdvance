using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// データベース上の従業員レコードの行ID（rowId）を表すValueObject
/// 【範囲】1以上（long.MaxValue以下）
/// 【責務】t_employees.row_id の管理と検証
/// </summary>
public sealed class EmployeeRowId : PrimitiveValueObject<long>, IEquatable<EmployeeRowId>
{
    /// <summary>
    /// 従業員行IDの最小有効値
    /// </summary>
    public const long MinValue = 1L;

    /// <summary>
    /// 従業員行IDの値を取得する
    /// </summary>
    public long Value => ValueField;

    /// <summary>
    /// 指定された long 値からEmployeeRowIdを生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="value">従業員行ID（1以上）</param>
    /// <remarks>Validate は、基礎クラスのコンストラクタで自動実行される</remarks>
    private EmployeeRowId(long value) : base(value, true)
    {
    }

    /// <summary>
    /// 指定された long 値からEmployeeRowIdのインスタンスを生成する（推奨: Domain層での生成方式）
    /// 【責務】long値から従業員行IDを表現する
    /// </summary>
    /// <param name="value">従業員行ID（1以上）</param>
    /// <returns>指定された行IDのEmployeeRowIdのインスタンス</returns>
    /// <exception cref="ArgumentOutOfRangeException">0以下の値</exception>
    public static EmployeeRowId From(long value) => new(value);

    /// <summary>
    /// 指定された long 値からEmployeeRowIdのインスタンスの生成を試みる（型安全版）
    /// 【責務】null安全に EmployeeRowId を生成する（Domain層での生成方式）
    /// </summary>
    /// <param name="input">従業員行ID（null許容）</param>
    /// <param name="result">生成されたEmployeeRowIdのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
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
    /// 【責務】DB から読み込んだ long から EmployeeRowId を復元
    /// </summary>
    /// <param name="value">DB の employee.row_id 値</param>
    /// <param name="result">生成されたEmployeeRowIdのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
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
    /// 従業員行IDの文字列表現を取得する
    /// 【責務】数値を文字列に変換
    /// </summary>
    /// <returns>数値文字列（例："12345"）</returns>
    public override string ToString() => Value.ToString();

    /// <summary>
    /// 指定されたオブジェクトと等価かどうかを判定する
    /// </summary>
    /// <param name="obj">比較対象のオブジェクト</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public override bool Equals(object? obj) => Equals(obj as EmployeeRowId);

    /// <summary>
    /// 指定されたEmployeeRowIdと等価かどうかを判定する
    /// 【責務】指定されたEmployeeRowIdと等価かどうかを判定する
    /// </summary>
    /// <param name="other">比較対象のEmployeeRowId</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
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

    /// <summary>
    /// ハッシュコードを取得する
    /// 【責務】オブジェクトのハッシュコードを取得する
    /// </summary>
    /// <returns>オブジェクトのハッシュコード</returns>
    public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// 従業員行IDが有効か検証する
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
