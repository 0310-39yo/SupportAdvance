using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.Entities.DomainEvents;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Application.Events;

/// <summary>
/// ドメインイベント ディスパッチャー
///
/// イベントの型に基づいてハンドラーを解決し、非同期で処理する
/// </summary>
public class EventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public EventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task DispatchAsync(IDomainEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var eventType = @event.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

        var handlers = (IEnumerable<object>)_serviceProvider.GetService(
            typeof(IEnumerable<>).MakeGenericType(handlerType))!;

        if (handlers == null)
        {
            return;
        }

        var tasks = new List<Task>();
        foreach (var handler in handlers)
        {
            var handleMethod = handlerType.GetMethod("HandleAsync");
            if (handleMethod != null)
            {
                var task = (Task)handleMethod.Invoke(handler, new object[] { @event })!;
                tasks.Add(task);
            }
        }

        await Task.WhenAll(tasks);
    }
}
