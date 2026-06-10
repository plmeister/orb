using Orb.Core;

namespace Orb.Host;

public sealed class EventDispatcher(IEventBus bus, Kernel kernel, ILogger<EventDispatcher> logger) : BackgroundService
{
    private readonly IEventBus _bus = bus;
    private readonly Kernel _kernel = kernel;
    private readonly ILogger<EventDispatcher> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var e = await _bus.ReadAsync(ct);
                await _kernel.PublishAsync(e, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching event");
            }
        }
    }
}
