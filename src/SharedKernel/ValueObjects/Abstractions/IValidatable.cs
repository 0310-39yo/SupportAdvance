namespace SupportAdvance.SharedKernel.ValueObjects.Abstractions;

/// <summary>
/// 値の形式検証を行うインターフェース（Clock 不要）
///
/// 【責務】
///   値の基本的な制約をチェックします。
///   - 型の有効性（DateTime.MinValue/MaxValue など不正な値の除外）
///   - 基本的な範囲チェック（年齢 0～150 など）
///   - 形式チェック（大文字小文字制約など）
///
/// 【時間軸依存のルール】
///   時刻を基準とする検証（「未来日は不可」など）は、
///   このインターフェースではなく IValidateWithClock を使用します。
///
/// 【実装例】
///   - RespondentAge: 年齢が 0～150 の範囲内か
///   - RespondentAt: DateTime が MinValue/MaxValue でないか（形式）
///   - 時間ベースのビジネスルール（未来日など）は IValidateWithClock へ
/// </summary>
/// <typeparam name="TValue">検証対象の値の型</typeparam>
public interface IValidatable<TValue>
{
    /// <summary>
    /// 指定された値の形式を検証します。
    ///
    /// Clock に依存しない基本的な制約チェックのみを行います。
    /// 時間軸依存のビジネスロジック検証は IValidateWithClock.ValidateWithClock を使用してください。
    /// </summary>
    /// <param name="value">検証対象の値</param>
    /// <remarks>
    /// 【検証失敗時の動作】
    /// 値が無効な場合は例外をスロー（呼び出し側で処理が必須）
    ///   - ArgumentException: 一般的な検証失敗
    ///   - ArgumentOutOfRangeException: 値が許容範囲外
    ///   - FormatException: 形式が不正
    /// </remarks>
    /// <exception cref="System.ArgumentException">検証失敗</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">値が範囲外</exception>
    /// <exception cref="System.FormatException">形式が不正</exception>
    void Validate(TValue value);
}
