using SupportAdvance.Application.UseCases;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Samples.CarPreferences.Application.Repositories;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;
using SupportAdvance.Crosscutting.Logging;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Application.UseCases.GetUserPreferences;

/// <summary>
/// ユーザープリファレンス取得 UseCase
///
/// 【責務】ユーザーの好み情報を取得
/// 【用途】読み取り専用
/// </summary>
public class GetUserPreferencesUseCase
    : IUseCase<GetUserPreferencesRequest, GetUserPreferencesResponse>
{
    private readonly IUserPreferencesRepository _repository;
    private readonly IAppLogging<GetUserPreferencesUseCase> _logger;

    public GetUserPreferencesUseCase(
        IUserPreferencesRepository repository,
        IAppLogging<GetUserPreferencesUseCase> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetUserPreferencesResponse> ExecuteAsync(
        GetUserPreferencesRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            $"Fetching preferences for user {request.UserId}");

        var userId = RespondentPersonId.From(request.UserId);
        var preferences = await _repository.GetAsync(userId);

        if (preferences == null)
        {
            throw new InvalidOperationException(
                $"Preferences not found for user {request.UserId}");
        }

        return new GetUserPreferencesResponse
        {
            UserId = userId.ToString(),
            PreferredModel = preferences.PreferredModel?.ToString(),
            PreferredBodyType = preferences.PreferredBodyType?.ToString(),
            PrefersAutomatic = preferences.PrefersAutomatic,
            BudgetFrom = preferences.BudgetFrom?.Amount,
            BudgetTo = preferences.BudgetTo?.Amount,
            CreatedAt = new LocalDateTime(preferences.CreatedAt.Value),
            UpdatedAt = new LocalDateTime(preferences.UpdatedAt.Value!.Value)
        };
    }
}
