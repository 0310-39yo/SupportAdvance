using SupportAdvance.Application.UseCases;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Application.UseCases;

/// <summary>
/// 車の好みに関する処理のリクエストを表すクラス
/// </summary>
public class CarPreferencesRequest : IRequest
{
    /// <summary>
    /// 処理対象の名前
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 処理の詳細情報
    /// </summary>
    public string Details { get; set; } = string.Empty;
}
