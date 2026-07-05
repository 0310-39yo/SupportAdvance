using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;

/// <summary>
/// 回答者の個人IDを表すValueObject
/// </summary>
public sealed class RespondentPersonId : PrimitiveValueObject<int>, IOptionalValueObject<RespondentPersonId, int>,
    IEquatable<RespondentPersonId>
{
    /// <summary>
    /// 未設定状態のインスタンスを生成する
    /// 【責務】未設定状態の ValueObject を表現するインスタンスを作成する事
    /// </summary>
    /// <param name="isSet">未設定状態かどうかを示すフラグ</param>
    private RespondentPersonId(bool isSet) : base(isSet)
    {
    }

    /// <summary>
    /// 指定された整数値からインスタンスを生成する
    /// 【責務】正規済の値を受け取り、内部表現を初期化すること
    /// </summary>
    /// <param name="value">RespondentPersonIdの値</param>
    /// <param name="isSet">未設定状態かどうかを示すフラグ</param>
    private RespondentPersonId(int value, bool isSet) : base(value, isSet)
    {
    }

    /// <summary>
    /// 未設定状態のインスタンスを返す
    /// 【責務】未設定状態を表現する
    /// </summary>
    /// <returns>未設定状態のRespondentPersonIdのインスタンス</returns>
    public static RespondentPersonId Unset() => new(false);

    /// <summary>
    /// 指定された整数値からインスタンスを生成する
    /// 【責務】値を検証してインスタンスを生成する
    /// </summary>
    /// <param name="value">RespondentPersonIdの値</param>
    /// <returns>指定された整数値を持つRespondentPersonIdのインスタンス</returns>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static RespondentPersonId From(int value) => new(value, true);

    /// <summary>
    /// 指定された入力値を RespondentPersonId に変換することを試行する
    /// 【責務】nullを Unset として安全に変換する
    /// </summary>
    /// <param name="input">整数値</param>
    /// <param name="result">生成されたRespondentPersonIdのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFrom(int input, out RespondentPersonId result) => TryFrom((int?)input, out result);

    /// <summary>
    /// 設定された整数値の取得を試行
    /// 【責務】未設定状態を安全に処理する
    /// </summary>
    /// <param name="value">取得する値の格納先</param>
    /// <returns>値が設定されている場合はtrue、未設定の場合はfalse</returns>
    public new bool TryGetValue(out int value)
    {
        if (!IsSet)
        {
            value = 0;
            return false;
        }

        value = ValueField;
        return true;
    }

    /// <summary>
    /// 指定された入力値を RespondentPersonId に変換することを試行する
    /// 【責務】nullを Unset として安全に変換する
    /// </summary>
    /// <param name="input">RespondentPersonIdの値</param>
    /// <param name="result">生成されたRespondentPersonIdのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFrom(int? input, out RespondentPersonId result)
    {
        // nullの場合は未設定状態のRespondentPersonIdを返す(正常処理)
        if (!input.HasValue)
        {
            result = Unset();
            return true;
        }

        try
        {
            result = From(input.Value);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            // 入力値が不正な場合は、未設定状態のRespondentPersonIdを返す(異常処理)
            result = Unset();
            return false;
        }
    }

    /// <summary>
    /// 指定された RespondentPersonId インスタンスと現在のインスタンスが等価かどうかを判定する
    /// 【責務】２つの ValueObject が等価かどうかを判定する
    /// </summary>
    /// <param name="other">比較対象のRespondentPersonId</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public bool Equals(RespondentPersonId? other) => other is not null && Equals((ValueObject?)other);

    /// <summary>
    /// 指定されたオブジェクトと等価かどうかを判定する
    /// 【責務】指定されたオブジェクトと等価かどうかを判定する
    /// </summary>
    /// <param name="obj">比較対象のオブジェクト</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public override bool Equals(object? obj) => Equals(obj as RespondentPersonId);

    /// <summary>
    /// ハッシュコードを取得する
    /// 【責務】オブジェクトのハッシュコードを取得する
    /// </summary>
    /// <returns>オブジェクトのハッシュコード</returns>
    public override int GetHashCode() => HashCode.Combine(IsSet, ValueField);

    /// <summary>
    /// 等価性の比較に使用するコンポーネントを取得する
    /// 【責務】IsSetとValueFieldを返すことで、等価性の比較に使用するコンポーネントを提供する
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return IsSet;

        if (IsSet)
        {
            yield return ValueField;
        }
    }

    /// <summary>
    /// 正規済の整数値を保証する
    /// 【責務】業務ルールに基づく妥当性のチェック
    /// PersonIdは1000から9999の範囲である必要があるため、範囲外の値の場合は
    /// /// ArgumentOutOfRangeExceptionをスローする
    /// </summary>
    /// <param name="normalized">検証する整数値</param>
    /// <exception cref="ArgumentOutOfRangeException">値が有効な範囲外の場合にスローされる</exception>
    protected override void Validate(int normalized)
    {
        base.Validate(normalized);

        const int minId = 1000;
        const int maxId = 9999;

        if (normalized < minId || normalized > maxId)
        {
            throw new ArgumentOutOfRangeException(
                nameof(normalized),
                normalized,
                $"RespondentPersonId must be between {minId} and {maxId}.");
        }
    }
}
