using SupportAdvance.Application.UseCases;
using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Application.UseCases.CreateUserPreferences;

/// <summary>
/// ユーザープリファレンス作成レスポンス
/// </summary>
public class CreateUserPreferencesResponse : IResponse
{
    /// <summary>ユーザーID</summary>
    public string UserId { get; set; } = null!;

    /// <summary>作成日時（JST）</summary>
    public LocalDateTime CreatedAt { get; set; }
}
