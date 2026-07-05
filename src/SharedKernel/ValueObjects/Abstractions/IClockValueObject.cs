using SupportAdvance.Common.Clocks;

namespace SupportAdvance.SharedKernel.ValueObjects;

/// <summary>
/// 指定された時刻を基準として、値を検証するためのインターフェイス
/// </summary>
/// <typeparam name="TValue">検証対象の値の型</typeparam>
public interface IClockValueObject<in TValue>
{
    /// <summary>
    /// 指定された時刻を基準として、値を検証する。
    /// </summary>
    /// <param name="value">検証対象の値</param>
    /// <param name="clock">現在時刻を供給する <see cref="IClock" /> インスタンス</param>
    /// <exception cref="ArgumentException">検証失敗時（未来日など）</exception>
    void ValidateWithClock(TValue value, IClock clock);
}
