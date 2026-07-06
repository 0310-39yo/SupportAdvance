using SupportAdvance.Common.Clocks;

namespace SupportAdvance.SharedKernel.ValueObjects.Abstractions;

/// <summary>
/// IClock を使用するオプション値オブジェクトのインターフェース
/// IValidateWithClock と IValidatable を継承し、clock-aware な検証と optional な振る舞いを提供する
/// IOptionalValueObject は継承しない（clock パラメータなしの From/TryFrom が不要なため）
/// </summary>
/// <typeparam name="TSelf">オプション値オブジェクト自身の型</typeparam>
/// <typeparam name="TValue">内部値の型</typeparam>
public interface IOptionalValidateWithClock<TSelf, TValue> :
    IValidateWithClock<TValue>,
    IValidatable<TValue>
    where TSelf : IOptionalValidateWithClock<TSelf, TValue>
{
    /// <summary>
    /// 内部値を取得する
    /// 値が設定されている場合は true を返し、<paramref name="value" /> に値を設定する
    /// 値が未設定の場合は false を返す
    /// </summary>
    /// <param name="value">取得した内部値（out パラメータ）</param>
    /// <returns>値が設定されている場合は true、未設定の場合は false</returns>
    bool TryGetValue(out TValue value);

    /// <summary>
    /// 未設定インスタンスを返す
    /// </summary>
    /// <returns>未設定の <typeparamref name="TSelf" /> インスタンス</returns>
    static abstract TSelf Unset();

    /// <summary>
    /// IClock を使用して検証を行いながら、指定された値からインスタンスを生成します。
    /// </summary>
    /// <param name="value">検証対象の値（正規化済み）</param>
    /// <param name="clock">現在時刻を供給する IClock インスタンス</param>
    /// <returns>検証済みの設定済み状態の ValueObject インスタンス（非null）</returns>
    /// <remarks>
    /// <para>入力検証失敗時は例外を投げる（ArgumentException/ArgumentOutOfRangeException/FormatException等）</para>
    /// <para>正規化（Normalize）と検証（Validate）、IClock検証（ValidateWithClock）をこのメソッド内で実施</para>
    /// <para>返却値は必ずIsSet=true の有効なインスタンス</para>
    /// </remarks>
    /// <exception cref="System.ArgumentException">入力検証失敗</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">範囲外</exception>
    /// <exception cref="System.FormatException">フォーマット失敗</exception>
    static abstract TSelf From(TValue value, IClock clock);

    /// <summary>
    /// IClock を使用して、指定された値からインスタンスの生成を試みます。
    /// <paramref name="input" /> が null の場合は Unset を返して true を返します（正常）。
    /// 値が無効な場合は Unset を返して false を返します（エラー）。
    /// API 層で null 許容の入力を処理する場合に使用します。
    /// </summary>
    /// <param name="input">生成に使用する値（nullable）</param>
    /// <param name="clock">現在時刻を供給する IClock インスタンス</param>
    /// <param name="result">生成結果を受け取る out パラメータ</param>
    /// <returns>
    /// 生成に成功した場合、または null で Unset に変換した場合は true; 値が無効な場合は false
    /// </returns>
    /// <remarks>
    /// <para>エラーを投げない安全な生成メソッド</para>
    /// <para>input が null: true 返却、result は Unset（正常な未設定状態）</para>
    /// <para>From 成功: true 返却、result は設定済みインスタンス</para>
    /// <para>From 失敗（例外）: false 返却、result は Unset（無効な入力）</para>
    /// <para>外部（JSON/API/DB）からの null 入力を許容するため TValue? を使用</para>
    /// </remarks>
    static abstract bool TryFrom(TValue? input, IClock clock, out TSelf result);
}
