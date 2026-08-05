using SupportAdvance.Application.UseCases;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Samples.CarPreferences.Application.Events;
using SupportAdvance.Contexts.Samples.CarPreferences.Application.Repositories;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.Entities;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;
using SupportAdvance.Crosscutting.Logging;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Application.UseCases.UpdateUserPreferences;

/// <summary>
/// ユーザープリファレンス更新 UseCase
///
/// 【責務】ユーザーの好みを更新し、イベント処理と永続化を行う
/// </summary>
public class UpdateUserPreferencesUseCase
    : IUseCase<UpdateUserPreferencesRequest, UpdateUserPreferencesResponse>
{
    private readonly IUserPreferencesRepository _repository;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IAppLogging<UpdateUserPreferencesUseCase> _logger;
    private readonly IClock _clock;

    public UpdateUserPreferencesUseCase(
        IUserPreferencesRepository repository,
        IEventDispatcher eventDispatcher,
        IAppLogging<UpdateUserPreferencesUseCase> logger,
        IClock clock)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<UpdateUserPreferencesResponse> ExecuteAsync(
        UpdateUserPreferencesRequest request)
    {
        // [1] パラメータ検証
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            $"Updating preferences for user {request.UserId}");

        // [2] Entity 取得
        var userId = RespondentPersonId.From(request.UserId);
        var preferences = await _repository.GetAsync(userId);

        if (preferences == null)
        {
            throw new InvalidOperationException(
                $"Preferences not found for user {request.UserId}");
        }

        // [3] Domain層呼び出し（更新）
        CarModel? model = null;
        if (request.PreferredModelValue.HasValue)
        {
            model = CarModel.From(request.PreferredModelValue.Value);
        }

        BodyType? bodyType = null;
        if (!string.IsNullOrEmpty(request.PreferredBodyType))
        {
            if (!BodyType.TryFrom(request.PreferredBodyType, out bodyType))
            {
                throw new ArgumentException(
                    $"Invalid BodyType: {request.PreferredBodyType}",
                    nameof(request.PreferredBodyType));
            }
        }

        Money? budgetFrom = null;
        if (request.BudgetFrom.HasValue)
        {
            budgetFrom = Money.From(request.BudgetFrom.Value);
        }

        Money? budgetTo = null;
        if (request.BudgetTo.HasValue)
        {
            budgetTo = Money.From(request.BudgetTo.Value);
        }

        preferences.UpdatePreferences(
            model,
            bodyType,
            request.PrefersAutomatic,
            budgetFrom,
            budgetTo,
            _clock);

        // [4] イベント処理
        var domainEvents = preferences.DomainEvents;
        foreach (var @event in domainEvents)
        {
            await _eventDispatcher.DispatchAsync(@event);
        }

        // [5] 永続化
        await _repository.UpdateAsync(preferences);

        _logger.LogInformation(
            $"Preferences updated for user {request.UserId}");

        return new UpdateUserPreferencesResponse
        {
            UserId = userId.ToString(),
            UpdatedAt = new LocalDateTime(preferences.UpdatedAt.Value!.Value)
        };
    }
}
