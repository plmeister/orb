using Orb.Abstractions;

namespace Orb.Core;

public interface IKernelEmitter
{
    Task EmitActionAsync(OrbAction a, CancellationToken ct);
    Task EmitEventAsync(OrbEvent e, CancellationToken ct);
}
