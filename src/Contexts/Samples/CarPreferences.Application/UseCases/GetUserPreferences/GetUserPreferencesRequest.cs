using SupportAdvance.Application.UseCases;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Application.UseCases.GetUserPreferences;

/// <summary>
/// ユーザープリファレンス取得リクエスト
/// </summary>
public class GetUserPreferencesRequest : IRequest
{
    /// <summary>ユーザーID（1000～9999）</summary>
    public int UserId { get; set; }
}
