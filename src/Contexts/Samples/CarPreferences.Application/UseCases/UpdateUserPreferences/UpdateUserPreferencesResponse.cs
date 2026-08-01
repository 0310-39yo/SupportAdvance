using SupportAdvance.Application.UseCases;
using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Application.UseCases.UpdateUserPreferences;

/// <summary>
/// ユーザープリファレンス更新レスポンス
/// </summary>
public class UpdateUserPreferencesResponse : IResponse
{
    /// <summary>ユーザーID</summary>
    public string UserId { get; set; } = null!;

    /// <summary>更新日時（JST）</summary>
    public LocalDateTime UpdatedAt { get; set; }
}
