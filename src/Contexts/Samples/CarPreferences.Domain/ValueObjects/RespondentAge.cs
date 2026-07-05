using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;

/// <summary>
/// 回答者の年齢を表すValueObject
/// </summary>
public sealed class RespondentAge : PrimitiveValueObject<int>, IOptionalValueObject<RespondentAge, int>,
    IEquatable<RespondentAge>
{
    /// <summary>
    /// 未設定状態のRespondentAgeのインスタンスを生成する
    /// 【責務】未設定状態のRespondentAgeを表現する
    /// </summary>
    /// <param name="isSet">未設定状態かどうかを示すフラグ</param>
    private RespondentAge(bool isSet) : base(isSet)
    {
    }

    /// <summary>
    /// 指定された整数値からRespondentAgeのインスタンスを生成する
    /// 【責務】指定された整数値を持つRespondentAgeを表現する
    /// </summary>
    /// <param name="value">整数値</param>
    /// <param name="isSet">未設定状態かどうかを示すフラグ</param>
    private RespondentAge(int value, bool isSet) : base(value, isSet)
    {
    }

    /// <summary>
    /// 指定された整数値からRespondentAgeのインスタンスを生成する
    /// 【責務】指定された整数値を持つRespondentAgeを表現する
    /// </summary>
    /// <param name="value">整数値</param>
    /// <returns>指定された整数値を持つRespondentAgeのインスタンス</returns>
    /// <remarks>Validate は,基底クラスのコンストラクタで自動実行される</remarks>
    public static RespondentAge From(int value) => new(value, true);

    /// <summary>
    /// 未設定状態のRespondentAgeのインスタンスを生成する
    /// 【責務】未設定状態のRespondentAgeを表現する
    /// </summary>
    /// <returns>未設定状態のRespondentAgeのインスタンス</returns>
    public static RespondentAge Unset() => new(false);

    /// <summary>
    /// 指定された整数値からRespondentAgeのインスタンスを生成する
    /// 【責務】指定された整数値を持つRespondentAgeを表現する
    /// </summary>
    /// <param name="input">整数値</param>
    /// <param name="result">生成されたRespondentAgeのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFrom(int? input, out RespondentAge result)
    {
        // nullの場合は未設定状態のRespondentAgeを返す(正常処理)
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
            // 入力値が不正な場合は、未設定状態のRespondentAgeを返す(異常処理)
            result = Unset();
            return false;
        }
    }

    /// <summary>
    /// 指定された整数値からRespondentAgeのインスタンスを生成する
    /// 【責務】指定された整数値を持つRespondentAgeを表現する
    /// </summary>
    /// <param name="input">整数値</param>
    /// <param name="result">生成されたRespondentAgeのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
    public static bool TryFrom(int input, out RespondentAge result) => TryFrom((int?)input, out result);

    /// <summary>
    /// 保持する値を取得する
    /// 【責務】保持する値を取得する
    /// </summary>
    /// <param name="value">取得する値の格納先</param>
    /// <returns>値が設定されている場合はtrue、未設定の場合はfalse</returns>
    public new bool TryGetValue(out int value)
    {
        if(!IsSet)
        {
            value = 0;
            return false;
        }

        value = ValueField;
        return true;
    }

    /// <summary>
    /// 指定されたオブジェクトと等価かどうかを判定する
    /// 【責務】指定されたオブジェクトと等価かどうかを判定する
    /// </summary>
    /// <param name="obj">比較対象のオブジェクト</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public override bool Equals(object? obj) => Equals(obj as RespondentAge);

    /// <summary>
    /// 指定されたRespondentAgeと等価かどうかを判定する
    /// 【責務】指定されたRespondentAgeと等価かどうかを判定する
    /// </summary>
    /// <param name="other">比較対象のRespondentAge</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public bool Equals(RespondentAge? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return IsSet == other.IsSet && ValueField == other.ValueField;
    }

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
    /// 正規化済み値の検証を行う
    /// 【責務】業務ルールに基づく値の妥当性のチェック
    /// Ageは0歳以上150歳以下である必要があるため、範囲外の値の場合は
    /// ArgumentOutOfRangeExceptionをスローする     
    /// </summary>
    /// <param name="normalized">正規化済みの値</param>
    /// <exception cref="ArgumentOutOfRangeException">値が有効な範囲外の場合にスローされる</exception>
    protected override void Validate(int normalized)
    {
        base.Validate(normalized);

        const int minAge = 0;
        const int maxAge = 150;

        if (normalized < minAge || normalized > maxAge)
        {
            throw new ArgumentOutOfRangeException(nameof(normalized), $"Age must be between {minAge} and {maxAge}.");
        }
    }
}
