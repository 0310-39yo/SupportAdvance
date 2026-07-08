using SupportAdvance.SharedKernel.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;

/// <summary>
/// 回答者の個人IDを表すValueObject
/// </summary>
public sealed class RespondentPersonId : PrimitiveValueObject<int>, IOptionalValueObject<RespondentPersonId, int>,
    IEquatable<RespondentPersonId>
{
    /// <summary>
    /// 未設定状態のインスタンスを生成するコンストラクタ
    /// </summary>
    /// <remarks>
    /// 【責務】未設定状態を表現するインスタンスを構築する
    /// </remarks>
    /// <param name="isSet">未設定状態かどうかを示すフラグ</param>
    private RespondentPersonId(bool isSet) : base(isSet)
    {
    }

    /// <summary>
    /// 指定された整数値からインスタンスを生成するコンストラクタ
    /// </summary>
    /// <remarks>
    /// 【責務1】指定された整数値を正規化・検証してインスタンスを構築する
    /// 【責務2】Validate メソッドは基底クラスのコンストラクタで自動実行される
    /// </remarks>
    /// <param name="value">個人ID の値</param>
    /// <param name="isSet">未設定状態かどうかを示すフラグ</param>
    private RespondentPersonId(int value, bool isSet) : base(value, isSet)
    {
    }

    /// <summary>
    /// 保持する整数値を取得する
    /// </summary>
    /// <remarks>
    /// IsSet=true の場合は整数値を返し、IsSet=false の場合は null を返す
    /// </remarks>
    public int? Value => IsSet ? ValueField : null;

    /// <summary>
    /// 未設定状態のインスタンスを生成する
    /// </summary>
    /// <remarks>
    /// 【責務】未設定状態を表現するインスタンスを返す
    /// </remarks>
    /// <returns>未設定状態のインスタンス</returns>
    public static RespondentPersonId Unset() => new(false);

    /// <summary>
    /// 指定された整数値からインスタンスを生成する
    /// </summary>
    /// <remarks>
    /// 【責務】指定された整数値を検証してインスタンスを生成する（Validate は基底クラスのコンストラクタで自動実行）
    /// </remarks>
    /// <param name="value">個人ID の値</param>
    /// <returns>検証済みで設定状態のインスタンス</returns>
    /// <exception cref="ArgumentOutOfRangeException">値が有効な範囲外の場合</exception>
    public static RespondentPersonId From(int value) => new(value, true);

    /// <summary>
    /// 指定された整数値からインスタンスの生成を試みる（非 nullable 版）
    /// </summary>
    /// <remarks>
    /// 【責務】nullable 版の TryFrom を呼び出す便利メソッド
    /// </remarks>
    /// <param name="input">個人ID の値</param>
    /// <param name="result">生成結果を受け取る out パラメータ</param>
    /// <returns>生成に成功した場合は true；検証失敗時は false</returns>
    public static bool TryFrom(int input, out RespondentPersonId result) => TryFrom((int?)input, out result);

    /// <summary>
    /// 保持する値を取得する
    /// </summary>
    /// <remarks>
    /// 【責務】未設定状態を安全に処理して値を返す
    /// </remarks>
    /// <param name="value">取得する値の格納先</param>
    /// <returns>値が設定されている場合は true、未設定の場合は false</returns>
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
    /// 指定された整数値からインスタンスの生成を試みる（nullable 版）
    /// </summary>
    /// <remarks>
    /// 【責務】外部入力を安全に処理する（null は未設定状態に、検証失敗時も未設定状態に変換）
    /// </remarks>
    /// <param name="input">個人ID の値（null 許容）</param>
    /// <param name="result">生成結果を受け取る out パラメータ</param>
    /// <returns>生成に成功した場合、または null 入力を Unset に変換した場合は true；検証失敗時は false</returns>
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
    /// 指定された RespondentPersonId インスタンスと等価かどうかを判定する
    /// </summary>
    /// <param name="other">比較対象の RespondentPersonId</param>
    /// <returns>等価である場合は true、そうでない場合は false</returns>
    public bool Equals(RespondentPersonId? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return IsSet == other.IsSet && ValueField == other.ValueField;
    }

    /// <summary>
    /// 指定されたオブジェクトと等価かどうかを判定する
    /// </summary>
    /// <param name="obj">比較対象のオブジェクト</param>
    /// <returns>等価である場合は true、そうでない場合は false</returns>
    public override bool Equals(object? obj) => Equals(obj as RespondentPersonId);

    /// <summary>
    /// ハッシュコードを取得する
    /// </summary>
    /// <remarks>
    /// 【責務】IsSet と ValueField に基づくハッシュコードを計算する
    /// </remarks>
    /// <returns>オブジェクトのハッシュコード</returns>
    public override int GetHashCode() => HashCode.Combine(IsSet, ValueField);

    /// <summary>
    /// 等価性判定のための値コンポーネントを返す（IsSet を除く）
    /// </summary>
    /// <remarks>
    /// IsSet は ValueObject.GetEqualityComponents で自動的に先頭に付加される
    /// </remarks>
    /// <returns>ValueField（IsSet = true の場合）を含むコンポーネント列</returns>
    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet)
        {
            yield return ValueField;
        }
    }

    /// <summary>
    /// 値の妥当性を検証する
    /// </summary>
    /// <remarks>
    /// 【責務】個人ID が 1000～9999 の有効な範囲かをチェックする
    /// </remarks>
    /// <param name="normalized">検証対象の値</param>
    /// <exception cref="ArgumentOutOfRangeException">値が有効な範囲外の場合</exception>
    public override void Validate(int normalized)
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
