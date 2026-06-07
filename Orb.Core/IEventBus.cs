using Orb.Abstractions;

namespace Orb.Core;

public interface IEventBus
{
    ValueTask PublishAsync(OrbEvent e, CancellationToken ct);
    ValueTask<OrbEvent> ReadAsync(CancellationToken ct);
}
