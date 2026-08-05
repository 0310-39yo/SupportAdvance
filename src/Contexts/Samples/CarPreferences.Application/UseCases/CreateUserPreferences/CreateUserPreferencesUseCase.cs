using SupportAdvance.Application.UseCases;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Samples.CarPreferences.Application.Events;
using SupportAdvance.Contexts.Samples.CarPreferences.Application.Repositories;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.DomainEvents;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.Entities;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;
using SupportAdvance.Crosscutting.Logging;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Application.UseCases.CreateUserPreferences;

/// <summary>
/// ユーザープリファレンス作成 UseCase
///
/// 【責務】
/// - 新規ユーザーの初期好み設定を Domain層で作成
/// - イベント処理と永続化を行う
/// 【シーン】ユーザー登録直後、アンケート完了時
/// </summary>
public class CreateUserPreferencesUseCase
    : IUseCase<CreateUserPreferencesRequest, CreateUserPreferencesResponse>
{
    private readonly IUserPreferencesRepository _repository;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IAppLogging<CreateUserPreferencesUseCase> _logger;
    private readonly IClock _clock;

    public CreateUserPreferencesUseCase(
        IUserPreferencesRepository repository,
        IEventDispatcher eventDispatcher,
        IAppLogging<CreateUserPreferencesUseCase> logger,
        IClock clock)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<CreateUserPreferencesResponse> ExecuteAsync(
        CreateUserPreferencesRequest request)
    {
        // [1] パラメータ検証
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            $"Creating preferences for user {request.UserId}");

        // [2] ValueObject 作成
        var userId = RespondentPersonId.From(request.UserId);
        var respondedAt = RespondentAt.From(
            request.RespondedAt,
            _clock);

        // [3] Domain層呼び出し（Entity 作成）
        // 注：新規作成時は rowId が未定（DB挿入後に採番）
        var preferences = new UserPreferences(userId, respondedAt, _clock);

        // 初期設定を適用
        if (request.PreferredModelValue.HasValue)
        {
            var model = CarModel.From(request.PreferredModelValue.Value);
            preferences.UpdatePreferredModel(model, _clock);
        }

        if (!string.IsNullOrEmpty(request.PreferredBodyType) &&
            BodyType.TryFrom(request.PreferredBodyType, out var bodyType))
        {
            preferences.UpdateBodyType(bodyType, _clock);
        }

        if (request.BudgetFrom.HasValue || request.BudgetTo.HasValue)
        {
            var from = request.BudgetFrom.HasValue ? Money.From(request.BudgetFrom.Value) : null;
            var to = request.BudgetTo.HasValue ? Money.From(request.BudgetTo.Value) : null;
            preferences.UpdateBudget(from, to, _clock);
        }

        // [4] イベント処理
        var domainEvents = preferences.DomainEvents;
        foreach (var @event in domainEvents)
        {
            await _eventDispatcher.DispatchAsync(@event);
        }

        // [5] 永続化
        await _repository.AddAsync(preferences);

        // 注：PreferencesCreatedEvent は新規作成時に rowId が未定（DB挿入後採番）
        // ため、ここでは発行しない。必要に応じて別途メカニズムを検討

        _logger.LogInformation(
            $"Preferences created for user {request.UserId}");

        return new CreateUserPreferencesResponse
        {
            UserId = userId.ToString(),
            CreatedAt = new LocalDateTime(preferences.CreatedAt.Value)
        };
    }
}
