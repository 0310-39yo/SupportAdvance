namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Identifiers;

using SupportAdvance.SharedKernel.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

/// <summary>
/// 従業員コードを表すValueObject（従業員区分+従業員番号の複合値）
/// 【値】EmployeeDivision + EmployeeNumber
/// 【表示】"M1234" 形式（区分+番号、スペースなし）
/// 【責務】複合値の管理、範囲検証、表示形式の提供
/// </summary>
public sealed class EmployeeCode : ValueObject, IEquatable<EmployeeCode>
{
    /// <summary>
    /// 従業員区分を取得する
    /// </summary>
    public EmployeeDivision Division { get; }

    /// <summary>
    /// 従業員番号を取得する
    /// </summary>
    public EmployeeNumber Number { get; }

    /// <summary>
    /// 指定された Division と Number からEmployeeCodeを生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="division">従業員区分</param>
    /// <param name="number">従業員番号</param>
    private EmployeeCode(EmployeeDivision division, EmployeeNumber number)
    {
        Division = division;
        Number = number;
    }

    /// <summary>
    /// 指定された Division と Number からEmployeeCodeのインスタンスを生成する（推奨: Domain層での生成方式）
    /// 【責務】Division と Number の組み合わせが有効か検証して生成
    /// </summary>
    /// <param name="division">従業員区分</param>
    /// <param name="number">従業員番号</param>
    /// <returns>指定された組み合わせのEmployeeCodeのインスタンス</returns>
    /// <exception cref="ArgumentException">無効な組み合わせ（範囲チェック失敗）</exception>
    public static EmployeeCode From(EmployeeDivision division, EmployeeNumber number)
    {
        ValidateDivisionAndNumber(division, number);
        return new(division, number);
    }

    /// <summary>
    /// 指定された Division と Number からEmployeeCodeのインスタンスの生成を試みる（型安全版）
    /// 【責務】null安全に EmployeeCode を生成する
    /// </summary>
    /// <param name="division">従業員区分</param>
    /// <param name="number">従業員番号</param>
    /// <param name="result">生成されたEmployeeCodeのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFrom(EmployeeDivision division, EmployeeNumber number, out EmployeeCode result)
    {
        result = null!;

        if (division == null || number == null)
        {
            return false;
        }

        try
        {
            result = From(division, number);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    /// <summary>
    /// 指定された文字列からEmployeeCodeのインスタンスの生成を試みる（文字列パース版）
    /// 【責務】"M1234" 形式の文字列から EmployeeCode を復元する（ログイン認証で使用）
    /// </summary>
    /// <param name="input">"M1234" または "M12345" 形式の文字列</param>
    /// <param name="result">生成されたEmployeeCodeのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryParse(string? input, out EmployeeCode result)
    {
        result = null!;

        // null または空文字列は失敗
        if (string.IsNullOrEmpty(input) || input.Length < 2)
        {
            return false;
        }

        try
        {
            // 最初の文字が区分
            char divisionChar = input[0];
            string numberStr = input[1..];

            // Division を復元
            if (!EmployeeDivision.TryFromDbValue(divisionChar.ToString(), out var division))
            {
                return false;
            }

            // Number を復元
            if (!int.TryParse(numberStr, out int numberInt))
            {
                return false;
            }

            if (!EmployeeNumber.TryFromDbValue(numberInt, out var number))
            {
                return false;
            }

            // 組み合わせ検証
            return TryFrom(division, number, out result);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 指定された Division (char) と Number (int) からEmployeeCodeのインスタンスの生成を試みる（DB値変換版）
    /// 【責務】DB から読み込んだ char/int から EmployeeCode を復元
    /// </summary>
    /// <param name="divisionChar">DB の employee_division 値（'M', 'T', 'C'）</param>
    /// <param name="numberInt">DB の employee_number 値</param>
    /// <param name="result">生成されたEmployeeCodeのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFromDbValues(char divisionChar, int numberInt, out EmployeeCode result)
    {
        result = null!;

        try
        {
            // Division を復元
            if (!EmployeeDivision.TryFromDbValue(divisionChar.ToString(), out var division))
            {
                return false;
            }

            // Number を復元
            if (!EmployeeNumber.TryFromDbValue(numberInt, out var number))
            {
                return false;
            }

            // 組み合わせ検証
            return TryFrom(division, number, out result);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 従業員コードの文字列表現を取得する（"M1234" 形式）
    /// 【責務】区分 + 番号をスペースなしで連結
    /// </summary>
    /// <returns>"M1234" または "M12345" 形式の文字列</returns>
    public override string ToString() => $"{Division.Value}{Number.Value}";

    /// <summary>
    /// 指定されたオブジェクトと等価かどうかを判定する
    /// 【責務】指定されたオブジェクトと等価かどうかを判定する
    /// </summary>
    /// <param name="obj">比較対象のオブジェクト</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public override bool Equals(object? obj) => Equals(obj as EmployeeCode);

    /// <summary>
    /// 指定されたEmployeeCodeと等価かどうかを判定する
    /// 【責務】指定されたEmployeeCodeと等価かどうかを判定する
    /// </summary>
    /// <param name="other">比較対象のEmployeeCode</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public bool Equals(EmployeeCode? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Division.Equals(other.Division) && Number.Equals(other.Number);
    }

    /// <summary>
    /// ハッシュコードを取得する
    /// 【責務】オブジェクトのハッシュコードを取得する
    /// </summary>
    /// <returns>オブジェクトのハッシュコード</returns>
    public override int GetHashCode() => HashCode.Combine(Division, Number);

    /// <summary>
    /// 等価性判定のための値コンポーネントを返す
    /// </summary>
    /// <returns>Division と Number を含むコンポーネント列</returns>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return Division;
        yield return Number;
    }

    /// <summary>
    /// 従業員区分と従業員番号の組み合わせが有効か検証する
    /// 【責務】区分ごとの有効番号範囲をチェック
    /// </summary>
    /// <param name="division">従業員区分</param>
    /// <param name="number">従業員番号</param>
    /// <exception cref="ArgumentException">無効な組み合わせ（範囲外）</exception>
    private static void ValidateDivisionAndNumber(EmployeeDivision division, EmployeeNumber number)
    {
        int num = number.Value;

        if (division.IsRegularEmployee)
        {
            // 従業員（M）: 1001～6999 か 10000～
            if (!((1001 <= num && num <= 6999) || num >= 10000))
            {
                throw new ArgumentException(
                    $"Regular employee number {num} must be in range 1001-6999 or 10000+.");
            }
        }
        else if (division.IsDispatched)
        {
            // 派遣社員（T）: 7500～7999 か 70000～
            if (!((7500 <= num && num <= 7999) || num >= 70000))
            {
                throw new ArgumentException(
                    $"Dispatched employee number {num} must be in range 7500-7999 or 70000+.");
            }
        }
        else if (division.IsContractor)
        {
            // 請負者（C）: 8000～8499 か 80000～
            if (!((8000 <= num && num <= 8499) || num >= 80000))
            {
                throw new ArgumentException(
                    $"Contractor number {num} must be in range 8000-8499 or 80000+.");
            }
        }
    }
}


