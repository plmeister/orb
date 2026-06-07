using Orb.Abstractions;

namespace Orb.Core;

public interface IActionChannel
{
    ValueTask WriteAsync(OrbAction action, CancellationToken ct);
    IAsyncEnumerable<OrbAction> ReadAllAsync(CancellationToken ct);
}
