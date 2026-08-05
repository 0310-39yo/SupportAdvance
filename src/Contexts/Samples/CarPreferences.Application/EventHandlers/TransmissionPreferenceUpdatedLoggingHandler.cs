using SupportAdvance.Contexts.Samples.CarPreferences.Domain.DomainEvents;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.SharedKernel.Entities.DomainEvents;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Application.EventHandlers;

/// <summary>
/// トランスミッション希望更新イベント：ログ記録
/// </summary>
public class TransmissionPreferenceUpdatedLoggingHandler
    : IDomainEventHandler<TransmissionPreferenceUpdatedEvent>
{
    private readonly IAppLogging<TransmissionPreferenceUpdatedLoggingHandler> _logger;

    public TransmissionPreferenceUpdatedLoggingHandler(
        IAppLogging<TransmissionPreferenceUpdatedLoggingHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task HandleAsync(TransmissionPreferenceUpdatedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var oldPref = @event.OldPreference ? "Auto" : "Manual";
        var newPref = @event.NewPreference ? "Auto" : "Manual";

        _logger.LogInformation(
            $"Transmission preference updated for event_id {@event.EventId}: " +
            $"{oldPref} -> {newPref}");

        return Task.CompletedTask;
    }
}
