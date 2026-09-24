namespace SupportAdvance.Common.Clocks;

/// <summary>
/// BusinessDayClockの状態ファイルの内容
/// </summary>
/// <param name="StartDate">保存時の基点日。設定の基点日と異なる場合、状態は破棄</param>
/// <param name="LastEndedDate">最後に終了した業務日。なしの場合は <see langword="null"/></param>
/// <param name="IsOn">保存時に ON 中だったかどうか。<see langword="true"/> のまま残っている場合は異常終了の跡</param>
/// <param name="SessionDate">ON したときの業務日。OFF 中は <see langword="null"/></param>
internal sealed record BusinessDayClockState(
    DateOnly StartDate,
    DateOnly? LastEndedDate,
    bool IsOn,
    DateOnly? SessionDate);
