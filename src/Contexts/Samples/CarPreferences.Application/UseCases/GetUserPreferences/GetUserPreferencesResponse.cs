using SupportAdvance.Application.UseCases;
using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Application.UseCases.GetUserPreferences;

/// <summary>
/// ユーザープリファレンス取得レスポンス
/// </summary>
public class GetUserPreferencesResponse : IResponse
{
    /// <summary>ユーザーID</summary>
    public string UserId { get; set; } = null!;

    /// <summary>希望車種</summary>
    public string? PreferredModel { get; set; }

    /// <summary>希望ボディタイプ</summary>
    public string? PreferredBodyType { get; set; }

    /// <summary>オートマ希望フラグ</summary>
    public bool PrefersAutomatic { get; set; }

    /// <summary>予算下限（万円）</summary>
    public decimal? BudgetFrom { get; set; }

    /// <summary>予算上限（万円）</summary>
    public decimal? BudgetTo { get; set; }

    /// <summary>作成日時（JST）</summary>
    public LocalDateTime CreatedAt { get; set; }

    /// <summary>更新日時（JST）</summary>
    public LocalDateTime UpdatedAt { get; set; }
}
