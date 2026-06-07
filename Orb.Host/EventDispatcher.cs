using Orb.Core;

namespace Orb.Host;

public sealed class EventDispatcher(IEventBus bus, Kernel kernel) : BackgroundService
{
    private readonly IEventBus _bus = bus;
    private readonly Kernel _kernel = kernel;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var e = await _bus.ReadAsync(ct);
            await _kernel.PublishAsync(e, ct);
        }
    }
}
