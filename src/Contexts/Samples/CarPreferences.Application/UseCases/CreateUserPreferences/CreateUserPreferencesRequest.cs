using SupportAdvance.Application.UseCases;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Application.UseCases.CreateUserPreferences;

/// <summary>
/// ユーザープリファレンス作成リクエスト
/// </summary>
public class CreateUserPreferencesRequest : IRequest
{
    /// <summary>ユーザーID（1000～9999）</summary>
    public int UserId { get; set; }

    /// <summary>回答日時（RespondentAt として解析）</summary>
    public LocalDateTime RespondedAt { get; set; }

    /// <summary>希望車種（CarModel の内部値）</summary>
    public int? PreferredModelValue { get; set; }

    /// <summary>希望ボディタイプ</summary>
    public string? PreferredBodyType { get; set; }

    /// <summary>予算下限（円）</summary>
    public decimal? BudgetFrom { get; set; }

    /// <summary>予算上限（円）</summary>
    public decimal? BudgetTo { get; set; }
}
