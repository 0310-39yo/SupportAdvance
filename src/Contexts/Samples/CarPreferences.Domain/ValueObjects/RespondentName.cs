using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;

/// <summary>
/// 回答者の名前を表すValueObject
/// </summary>
public sealed class RespondentName : PrimitiveValueObject<string>, IOptionalValueObject<RespondentName, string>,
    IEquatable<RespondentName>
{
    /// <summary>
    /// 未設定状態のインスタンスを生成するコンストラクタ
    /// </summary>
    /// <remarks>
    /// 【責務】未設定状態を表現するインスタンスを構築する
    /// </remarks>
    /// <param name="isSet">未設定状態かどうかを示すフラグ</param>
    private RespondentName(bool isSet) : base(isSet)
    {
    }

    /// <summary>
    /// 指定された文字列値からインスタンスを生成するコンストラクタ
    /// </summary>
    /// <remarks>
    /// 【責務1】指定された文字列値を正規化・検証してインスタンスを構築する
    /// 【責務2】Validate メソッドは基底クラスのコンストラクタで自動実行される
    /// </remarks>
    /// <param name="value">文字列値</param>
    /// <param name="isSet">未設定状態かどうかを示すフラグ</param>
    private RespondentName(string value, bool isSet) : base(value, isSet)
    {
    }

    /// <summary>
    /// 保持する文字列値を取得する
    /// </summary>
    /// <remarks>
    /// IsSet=true の場合は文字列値を返し、IsSet=false の場合は null を返す
    /// </remarks>
    public string? Value => IsSet ? ValueField : null;

    /// <summary>
    /// 指定された RespondentName インスタンスと等価かどうかを判定する
    /// </summary>
    /// <param name="other">比較対象の RespondentName</param>
    /// <returns>等価である場合は true、そうでない場合は false</returns>
    public bool Equals(RespondentName? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return IsSet == other.IsSet && ValueField == other.ValueField;
    }

    /// <summary>
    /// 未設定状態のインスタンスを生成する
    /// </summary>
    /// <remarks>
    /// 【責務】未設定状態を表現するインスタンスを返す
    /// </remarks>
    /// <returns>未設定状態のインスタンス</returns>
    public static RespondentName Unset() => new(false);

    /// <summary>
    /// 指定された文字列値からインスタンスを生成する
    /// </summary>
    /// <remarks>
    /// 【責務1】null チェックを実施する
    /// 【責務2】指定された文字列値を検証してインスタンスを生成する（Validate は基底クラスのコンストラクタで自動実行）
    /// </remarks>
    /// <param name="value">文字列値</param>
    /// <returns>検証済みで設定状態のインスタンス</returns>
    /// <exception cref="ArgumentNullException">値が null の場合</exception>
    /// <exception cref="ArgumentException">検証に失敗した場合</exception>
    public static RespondentName From(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new RespondentName(value, true);
    }

    /// <summary>
    /// 指定された文字列値からインスタンスの生成を試みる（nullable 版）
    /// </summary>
    /// <remarks>
    /// 【責務】外部入力を安全に処理する（null は未設定状態に、検証失敗時も未設定状態に変換）
    /// </remarks>
    /// <param name="input">文字列値（null 許容）</param>
    /// <param name="result">生成結果を受け取る out パラメータ</param>
    /// <returns>生成に成功した場合、または null 入力を Unset に変換した場合は true；検証失敗時は false</returns>
    public static bool TryFrom(string? input, out RespondentName result)
    {
        if (input is null)
        {
            result = Unset();
            return true;
        }

        try
        {
            result = From(input);
            return true;
        }
        catch (ArgumentException)
        {
            result = Unset();
            return false;
        }
    }

    /// <summary>
    /// 値の妥当性を検証する
    /// </summary>
    /// <remarks>
    /// 【責務1】空文字列でないかをチェックする
    /// 【責務2】文字列が 1～50 文字の範囲内かをチェックする
    /// </remarks>
    /// <param name="normalized">検証対象の値</param>
    /// <exception cref="ArgumentException">検証に失敗した場合</exception>
    public override void Validate(string normalized)
    {
        base.Validate(normalized);

        if (string.IsNullOrEmpty(normalized))
        {
            throw new ArgumentException("RespondentName cannot be empty.", nameof(normalized));
        }

        if (normalized.Length > 50)
        {
            throw new ArgumentException("RespondentName must be 50 characters or less.", nameof(normalized));
        }
    }

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
    /// ハッシュコードを取得する
    /// </summary>
    /// <remarks>
    /// 【責務】IsSet と ValueField に基づくハッシュコードを計算する
    /// </remarks>
    /// <returns>オブジェクトのハッシュコード</returns>
    public override int GetHashCode() => HashCode.Combine(IsSet, ValueField);

    /// <summary>
    /// 指定されたオブジェクトと等価かどうかを判定する
    /// </summary>
    /// <param name="obj">比較対象のオブジェクト</param>
    /// <returns>等価である場合は true、そうでない場合は false</returns>
    public override bool Equals(object? obj) => Equals(obj as RespondentName);
}
