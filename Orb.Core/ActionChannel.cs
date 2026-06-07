using System.Threading.Channels;
using Orb.Abstractions;

namespace Orb.Core;

public sealed class ActionChannel : IActionChannel
{
    private readonly Channel<OrbAction> _channel;

    public ActionChannel()
    {
        _channel = Channel.CreateUnbounded<OrbAction>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false }
        );
    }

    public ValueTask WriteAsync(OrbAction action, CancellationToken ct)
    {
        return _channel.Writer.WriteAsync(action, ct);
    }

    public IAsyncEnumerable<OrbAction> ReadAllAsync(CancellationToken ct)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }
}
