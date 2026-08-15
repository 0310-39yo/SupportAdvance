using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// 従業員種別区分を表すValueObject
/// 【値】M（従業員）、T（派遣社員）、C（請負者）
/// 【責務】固定値の管理と検証、日本語名の提供
/// </summary>
public sealed class BizDivision : EnumValueObject<char>, IEquatable<BizDivision>
{
    /// <summary>
    /// 従業員（M）の内部値定数
    /// </summary>
    public const char RegularEmployeeValue = 'M';

    /// <summary>
    /// 派遣社員（T）の内部値定数
    /// </summary>
    public const char DispatchedValue = 'T';

    /// <summary>
    /// 請負者（C）の内部値定数
    /// </summary>
    public const char ContractorValue = 'C';

    /// <summary>
    /// 指定された char 値からEmployeeTypeDivisionを生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="value">区分値（M, T, C）</param>
    private BizDivision(char value) : base(value)
    {
    }

    /// <summary>
    /// 従業員（M）を表すEmployeeTypeDivisionのインスタンスを生成する
    /// </summary>
    /// <returns>従業員区分のEmployeeTypeDivisionのインスタンス</returns>
    public static BizDivision RegularEmployee() => new(RegularEmployeeValue);

    /// <summary>
    /// 派遣社員（T）を表すEmployeeTypeDivisionのインスタンスを生成する
    /// </summary>
    /// <returns>派遣社員区分のEmployeeTypeDivisionのインスタンス</returns>
    public static BizDivision Dispatched() => new(DispatchedValue);

    /// <summary>
    /// 請負者（C）を表すEmployeeTypeDivisionのインスタンスを生成する
    /// </summary>
    /// <returns>請負者区分のEmployeeTypeDivisionのインスタンス</returns>
    public static BizDivision Contractor() => new(ContractorValue);

    /// <summary>
    /// 指定された char 値からEmployeeTypeDivisionのインスタンスを生成する
    /// 【責務】char値から区分を生成する（Domain層での生成方式）
    /// </summary>
    /// <param name="value">区分値（M, T, C）</param>
    /// <returns>指定された区分のEmployeeTypeDivisionのインスタンス</returns>
    public static BizDivision From(char value) => new(value);

    /// <summary>
    /// 指定された char? 値からEmployeeTypeDivisionのインスタンスの生成を試みる（型安全版）
    /// 【責務】null安全に BizDivision を生成する
    /// </summary>
    /// <param name="input">区分値（M, T, C、またはnull）</param>
    /// <param name="result">生成されたEmployeeTypeDivisionのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFrom(char? input, out BizDivision result)
    {
        result = null!;

        if (!input.HasValue)
        {
            return false;
        }

        try
        {
            result = From(input.Value);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    /// <summary>
    /// 指定された string 値からEmployeeTypeDivisionのインスタンスを生成する（Infrastructure層での型変換用）
    /// 【責務】DB から読み込んだ nvarchar を char に変換して BizDivision を生成
    /// </summary>
    /// <param name="value">区分値（M, T, C）</param>
    /// <returns>指定された区分のEmployeeTypeDivisionのインスタンス</returns>
    public static BizDivision FromDbValue(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length == 0)
        {
            throw new ArgumentException("BizDivision value cannot be empty.", nameof(value));
        }

        return new BizDivision(value[0]);
    }

    /// <summary>
    /// 指定された string? 値からEmployeeTypeDivisionのインスタンスの生成を試みる（NULL安全版、Infrastructure層での型変換用）
    /// 【責務】DB値から null安全に BizDivision を生成する（NULL は失敗）
    /// </summary>
    /// <param name="input">区分値（M, T, C、またはnull）</param>
    /// <param name="result">生成されたEmployeeTypeDivisionのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFromDbValue(string? input, out BizDivision result)
    {
        result = null!;

        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        try
        {
            result = FromDbValue(input);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    /// <summary>
    /// 保持する char 値を取得する
    /// 【責務】保持する値を取得する
    /// </summary>
    /// <returns>保持する char 値</returns>
    public char Value => ValueField;

    /// <summary>
    /// 区分が従業員（M）かどうかを判定する
    /// </summary>
    /// <returns>従業員の場合はtrue、そうでない場合はfalse</returns>
    public bool IsRegularEmployee => ValueField == RegularEmployeeValue;

    /// <summary>
    /// 区分が派遣社員（T）かどうかを判定する
    /// </summary>
    /// <returns>派遣社員の場合はtrue、そうでない場合はfalse</returns>
    public bool IsDispatched => ValueField == DispatchedValue;

    /// <summary>
    /// 区分が請負者（C）かどうかを判定する
    /// </summary>
    /// <returns>請負者の場合はtrue、そうでない場合はfalse</returns>
    public bool IsContractor => ValueField == ContractorValue;

    /// <summary>
    /// 指定されたオブジェクトと等価かどうかを判定する
    /// 【責務】指定されたオブジェクトと等価かどうかを判定する
    /// </summary>
    /// <param name="obj">比較対象のオブジェクト</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public override bool Equals(object? obj) => Equals(obj as BizDivision);

    /// <summary>
    /// 指定されたEmployeeTypeDivisionと等価かどうかを判定する
    /// 【責務】指定されたEmployeeTypeDivisionと等価かどうかを判定する
    /// </summary>
    /// <param name="other">比較対象のEmployeeTypeDivision</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public bool Equals(BizDivision? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return ValueField == other.ValueField;
    }

    /// <summary>
    /// ハッシュコードを取得する
    /// 【責務】オブジェクトのハッシュコードを取得する
    /// </summary>
    /// <returns>オブジェクトのハッシュコード</returns>
    public override int GetHashCode() => ValueField.GetHashCode();

    /// <summary>
    /// 区分値の検証を行う
    /// 【責務】業務ルールに基づく値の妥当性のチェック（M, T, C のいずれか）
    /// </summary>
    /// <param name="value">検証対象の区分値</param>
    /// <exception cref="ArgumentOutOfRangeException">値が M, T, C のいずれでもない場合にスローされる</exception>
    public override void Validate(char value)
    {
        if (value != RegularEmployeeValue && value != DispatchedValue && value != ContractorValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"BizDivision must be one of: '{RegularEmployeeValue}' (従業員), '{DispatchedValue}' (派遣社員), '{ContractorValue}' (請負者).");
        }
    }

    /// <summary>
    /// 区分の日本語名を取得する
    /// 【責務】区分値に対応する業務名称（表示名）を返す
    /// </summary>
    /// <returns>区分の日本語名</returns>
    protected override string GetDisplayName()
    {
        return ValueField switch
        {
            RegularEmployeeValue => "従業員",
            DispatchedValue => "派遣社員",
            ContractorValue => "請負者",
            _ => "不明"
        };
    }
}
