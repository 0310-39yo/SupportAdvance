using SupportAdvance.Contexts.Samples.CarPreferences.Domain.DomainEvents;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.SharedKernel.Entities.DomainEvents;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Application.EventHandlers;

/// <summary>
/// プリファレンス更新イベント：ログ記録
/// </summary>
public class PreferencesUpdatedLoggingHandler
    : IDomainEventHandler<PreferencesUpdatedEvent>
{
    private readonly IAppLogging<PreferencesUpdatedLoggingHandler> _logger;

    public PreferencesUpdatedLoggingHandler(
        IAppLogging<PreferencesUpdatedLoggingHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task HandleAsync(PreferencesUpdatedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        _logger.LogInformation(
            $"Preferences updated for event_id {@event.EventId}: " +
            $"Type={@event.ChangeType}, OldValue={@event.OldValue}, NewValue={@event.NewValue}");

        return Task.CompletedTask;
    }
}
