using System.Threading.Channels;
using eBike.Application.Common.Interfaces;

namespace eBike.Infrastructure.Messaging;

public class ChannelEventBus : IEventBus
{
    private readonly Channel<IEvent> _channel;

    public ChannelEventBus(int capacity = 1000)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            SingleWriter = false,
            SingleReader = false,
            FullMode = BoundedChannelFullMode.Wait
        };
        _channel = Channel.CreateBounded<IEvent>(options);
    }

    public async ValueTask PublishAsync<TEvent>(TEvent eventItem, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        await _channel.Writer.WriteAsync(eventItem, cancellationToken);
    }

    public IAsyncEnumerable<IEvent> ReadAllEventsAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
