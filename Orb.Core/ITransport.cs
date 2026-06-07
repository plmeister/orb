using Orb.Abstractions;

namespace Orb.Core;

public interface ITransport
{
    string Name { get; }
    Task StartAsync(CancellationToken ct);
    Task SendAsync(OrbAction action, CancellationToken ct);
}
