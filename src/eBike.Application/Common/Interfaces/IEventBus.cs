namespace eBike.Application.Common.Interfaces;

public interface IEvent
{
    DateTime OccurredOn { get; }
}

public interface IEventBus
{
    ValueTask PublishAsync<TEvent>(TEvent eventItem, CancellationToken cancellationToken = default)
        where TEvent : IEvent;

    IAsyncEnumerable<IEvent> ReadAllEventsAsync(CancellationToken cancellationToken = default);
}
