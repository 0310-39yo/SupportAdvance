namespace SupportAdvance.SharedKernel.ValueObjects.Abstractions;

/// <summary>
/// 値の検証を行うインターフェース
/// </summary>
/// <typeparam name="TValue">検証対象の値の型</typeparam>
public interface IValidatable<TValue>
{
    /// <summary>
    /// 指定された値の検証を行う
    /// </summary>
    /// <param name="value">検証対象の値</param>
    /// <remarks>
    /// 検証失敗時は例外を投げる（ArgumentException/ArgumentOutOfRangeException/FormatException等）
    /// </remarks>
    /// <exception cref="System.ArgumentException">検証失敗</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">範囲外</exception>
    /// <exception cref="System.FormatException">フォーマット失敗</exception>
    void Validate(TValue value);
}
