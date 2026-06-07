using System.Threading.Channels;
using Orb.Abstractions;

namespace Orb.Core;

public sealed class EventBus : IEventBus
{
    private readonly Channel<OrbEvent> _channel = Channel.CreateBounded<OrbEvent>(
        new BoundedChannelOptions(1000)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait,
        }
    );

    public ValueTask PublishAsync(OrbEvent e, CancellationToken ct)
    {
        return _channel.Writer.WriteAsync(e, ct);
    }

    public ValueTask<OrbEvent> ReadAsync(CancellationToken ct)
    {
        return _channel.Reader.ReadAsync(ct);
    }
}
