using Microsoft.Extensions.Logging;
using Orb.Abstractions;

namespace Orb.Core;

public sealed class KernelEmitter(
    IEventBus eventBus,
    IActionChannel actionChannel,
    ILogger<KernelEmitter> logger
) : IKernelEmitter
{
    private readonly IEventBus _eventBus = eventBus;
    private readonly IActionChannel _actionChannel = actionChannel;
    private readonly ILogger<KernelEmitter> _logger = logger;

    public async Task EmitEventAsync(OrbEvent e, CancellationToken ct)
    {
        _logger.LogDebug("Emitting event {EventType}", e.Type);

        await _eventBus.PublishAsync(e, ct);
    }

    public async Task EmitActionAsync(OrbAction a, CancellationToken ct)
    {
        _logger.LogDebug("Emitting action {ActionType}", a.Type);

        await _actionChannel.WriteAsync(a, ct);
    }
}
