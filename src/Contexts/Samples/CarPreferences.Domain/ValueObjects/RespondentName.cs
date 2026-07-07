using SupportAdvance.SharedKernel.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;

/// <summary>
/// 回答者の名前を表すValueObject
/// </summary>
public sealed class RespondentName : PrimitiveValueObject<string>, IOptionalValueObject<RespondentName, string>,
    IEquatable<RespondentName>
{
    /// <summary>
    /// 未設定状態のRespondentNameのインスタンスを生成する
    /// 【責務】未設定状態のRespondentNameを表現する
    /// </summary>
    /// <param name="isSet">未設定状態かどうかを示すフラグ </param>
    private RespondentName(bool isSet) : base(isSet)
    {
    }

    /// <summary>
    /// 指定された文字列値からRespondentNameのインスタンスを生成する
    /// 【責務】指定された文字列値を持つRespondentNameを表現する
    /// </summary>
    /// <param name="value">RespondentNameの文字列値</param>
    /// <param name="isSet">未設定状態かどうかを示すフラグ </param>
    private RespondentName(string value, bool isSet) : base(value, isSet)
    {
    }

    /// <summary>
    /// 保持する文字列値を読み取り専用で取得
    /// </summary>
    public string? Value => IsSet ? ValueField : null;

    /// <summary>
    /// 指定されたRespondentNameと等価かどうかを判定する
    /// </summary>
    /// <param name="other">比較対象のRespondentName</param>
    /// <returns>等価である場合はtrue、そうでない場合はfalse</returns>
    public bool Equals(RespondentName? other) => base.Equals(other);

    /// <summary>
    /// RespondentNameのインスタンスを生成する
    /// 【責務】未設定状態のRespondentNameを表現する
    /// </summary>
    /// <returns>未設定状態のRespondentNameのインスタンス</returns>
    public static RespondentName Unset() => new(false);

    /// <summary>
    /// RespondentNameのインスタンスを生成する
    /// 【責務】指定された文字列値を持つRespondentNameを表現する
    /// </summary>
    /// <param name="value">RespondentNameの文字列値</param>
    /// <returns>指定された文字列値を持つRespondentNameのインスタンス</returns>
    /// <remarks>Validate は,基底クラスのコンストラクタで自動実行される</remarks>
    public static RespondentName From(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new RespondentName(value, true);
    }

    /// <summary>
    /// 指定された文字列値からRespondentNameのインスタンスを生成する
    /// </summary>
    /// <param name="input">RespondentNameの文字列値</param>
    /// <param name="result">生成されたRespondentNameのインスタンス</param>
    /// <returns>生成に成功した場合はtrue、失敗した場合はfalse</returns>
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
    /// 名前の値を検証します
    /// 【検証内容】
    ///   - 空文字列でないこと
    ///   - 1～50文字の範囲内であること
    /// </summary>
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
}
