using Orb.Abstractions;

namespace Orb.Core;

public interface IEventSink
{
    Task PublishAsync(OrbEvent e, CancellationToken ct);
}
