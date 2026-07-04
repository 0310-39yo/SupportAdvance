using SupportAdvance.Application.UseCases;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Application.UseCases;

/// <summary>
/// 車の好みに関する処理のレスポンスを表すクラス
/// </summary>
public class CarPreferencesResponse : IResponse
{
    /// <summary>
    /// 処理結果のメッセージ
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 処理の実行時刻
    /// </summary>
    public DateTime ExecutedAt { get; set; }

    /// <summary>
    /// 処理が成功したか
    /// </summary>
    public bool IsSuccess { get; set; }
}
