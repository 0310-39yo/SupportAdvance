namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

using SupportAdvance.SharedKernel.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects.Abstractions;



/// <summary>
/// 従業員の通し番号を表すValueObject
/// 【範囲】1001以上（1000は管理者予約。EmployeeCodeで区分ごとの有効範囲が検証される）
/// 【表示】左0埋めで5桁（例："01234"）
/// 【責務】従業員番号の管理と検証、表示形式の提供
/// </summary>
public sealed class EmployeeNumber : PrimitiveValueObject<int>, IEquatable<EmployeeNumber>
{
    /// <summary>
    /// 従業員番号の最小有効値
    /// </summary>
    public const int MinValue = 1001;

    /// <summary>
    /// システム管理者用の予約番号
    /// </summary>
    public const int ReservedValue = 1000;

    /// <summary>
    /// 指定された int 値からEmployeeNumberを生成する（プライベートコンストラクタ）
    /// </summary>
    /// <param name="value">従業員番号（1001～8499）</param>
    /// <remarks>Validate は、基礎クラスのコンストラクタで自動実行される</remarks>
    private EmployeeNumber(int value) : base(value, true)
    {
    }

    /// <summary>
    /// 指定された int 値からEmployeeNumberのインスタンスを生成する（推奨: Domain層での生成方式）
    /// 【責務】int値から従業員番号を表現する
    /// </summary>
    /// <param name="value">従業員番号（1001～8499）</param>
    /// <returns>指定された番号のEmployeeNumberのインスタンス</returns>
    public static EmployeeNumber From(int value) => new(value);

    /// <summary>
    /// 指定された int? 値からEmployeeNumberのインスタンスの生成を試みる（型安全版）
    /// 【責務】null安全に EmployeeNumber を生成する（Domain層での生成方式）
    /// </summary>
    /// <param name="input">従業員番号（null 許容）</param>
    /// <param name="result">生成されたEmployeeNumberのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFrom(int? input, out EmployeeNumber result)
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
    /// 指定された int 値からEmployeeNumberのインスタンスを生成する（Infrastructure層での型変換用）
    /// 【責務】DB から読み込んだ int を EmployeeNumber に変換
    /// </summary>
    /// <param name="value">DB読み込み値</param>
    /// <returns>指定された番号のEmployeeNumberのインスタンス</returns>
    public static EmployeeNumber FromDbValue(int value) => new(value);

    /// <summary>
    /// 指定された int? 値からEmployeeNumberのインスタンスの生成を試みる（NULL安全版、Infrastructure層での型変換用）
    /// 【責務】DB値から null安全に EmployeeNumber を生成する（NULL は失敗）
    /// </summary>
    /// <param name="input">DB読み込み値（null 許容）</param>
    /// <param name="result">生成されたEmployeeNumberのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFromDbValue(int? input, out EmployeeNumber result)
    {
        result = null!;

        if (!input.HasValue)
        {
            return false;
        }

        try
        {
            result = FromDbValue(input.Value);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    /// <summary>
    /// 保持する int 値を取得する
    /// 【責務】保持する値を取得する
    /// </summary>
    /// <returns>保持する int 値</returns>
    public int Value => ValueField;

    /// <summary>
    /// 左0埋めで5桁の文字列表現を取得する
    /// 【責務】従業員番号を表示用フォーマットで返す（例："01234"）
    /// </summary>
    /// <returns>左0埋めで5桁の文字列（例："01234"）</returns>
    public override string ToString() => ValueField.ToString("D5");

    /// <summary>
    /// 指定されたオブジェクトと等価かどうかを判定する
    /// 【責務】指定されたオブジェクトと等価かどうかを判定する
    /// </summary>
    /// <param name="obj">比較対象のオブジェクト</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public override bool Equals(object? obj) => Equals(obj as EmployeeNumber);

    /// <summary>
    /// 指定されたEmployeeNumberと等価かどうかを判定する
    /// 【責務】指定されたEmployeeNumberと等価かどうかを判定する
    /// </summary>
    /// <param name="other">比較対象のEmployeeNumber</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public bool Equals(EmployeeNumber? other)
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
    /// 等価性判定のための値コンポーネントを返す（IsSet を除く）
    /// IsSet は ValueObject.GetEqualityComponents で自動的に先頭に付加される
    /// </summary>
    /// <returns>ValueField を含むコンポーネント列</returns>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return ValueField;
    }

    /// <summary>
    /// 正規化済み値の検証を行う
    /// 【責務】業務ルールに基づく値の妥当性のチェック
    /// </summary>
    /// <param name="normalized">正規化済みの int 値</param>
    /// <exception cref="ArgumentOutOfRangeException">値が有効な範囲でない場合にスローされる</exception>
    public override void Validate(int normalized)
    {
        base.Validate(normalized);

        // 1000 は管理者予約
        if (normalized == ReservedValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(normalized),
                normalized,
                $"EmployeeNumber {ReservedValue} is reserved for system administrator.");
        }

        // 1001 以上か確認（上限なし。区分ごとの範囲検証は EmployeeCode で実施）
        if (normalized < MinValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(normalized),
                normalized,
                $"EmployeeNumber must be {MinValue} or higher ({ReservedValue} is reserved).");
        }
    }
}



