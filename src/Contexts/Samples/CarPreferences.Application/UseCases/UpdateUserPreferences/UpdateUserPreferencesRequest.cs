using SupportAdvance.Application.UseCases;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Application.UseCases.UpdateUserPreferences;

/// <summary>
/// ユーザープリファレンス更新リクエスト
/// </summary>
public class UpdateUserPreferencesRequest : IRequest
{
    /// <summary>ユーザーID（1000～9999）</summary>
    public int UserId { get; set; }

    /// <summary>希望車種（null = 更新しない）</summary>
    public int? PreferredModelValue { get; set; }

    /// <summary>希望ボディタイプ（null = 更新しない）</summary>
    public string? PreferredBodyType { get; set; }

    /// <summary>オートマ希望フラグ（null = 更新しない）</summary>
    public bool? PrefersAutomatic { get; set; }

    /// <summary>予算下限（null = 更新しない）</summary>
    public decimal? BudgetFrom { get; set; }

    /// <summary>予算上限（null = 更新しない）</summary>
    public decimal? BudgetTo { get; set; }
}
