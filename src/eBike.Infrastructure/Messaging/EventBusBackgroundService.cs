using eBike.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace eBike.Infrastructure.Messaging;

public class EventBusBackgroundService : BackgroundService
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<EventBusBackgroundService> _logger;

    public EventBusBackgroundService(IEventBus eventBus, ILogger<EventBusBackgroundService> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("eBike In-Memory Event Bus Worker started.");

        try
        {
            await foreach (var eventItem in _eventBus.ReadAllEventsAsync(stoppingToken))
            {
                try
                {
                    _logger.LogInformation("Processing event: {EventType} occurred at {OccurredOn}", 
                        eventItem.GetType().Name, eventItem.OccurredOn);

                    // Future domain event handlers can be dynamically invoked here
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred processing event {EventType}", eventItem.GetType().Name);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Event Bus Worker shutting down gracefully.");
        }
    }
}
