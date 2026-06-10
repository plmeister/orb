using NSubstitute;
using Orb.Abstractions;
using Orb.Core;
using Orb.Transport.Console;

namespace Orb.Tests;

public class ConsoleTransportTests
{
    [Fact]
    public async Task SendAsync_does_not_throw_when_content_exists()
    {
        var bus = Substitute.For<IEventBus>();
        var transport = new ConsoleTransport(bus);

        var action = new OrbAction(
            Guid.NewGuid(), OrbActionType.SendMessage,
            "t1", "c1", "u1",
            new ReplyTarget("console", "t1", "c1"),
            new() { ["content"] = "hello" }
        );

        var ex = await Record.ExceptionAsync(() => transport.SendAsync(action, CancellationToken.None));
        Assert.Null(ex);
    }

    [Fact]
    public async Task SendAsync_does_not_throw_when_content_missing()
    {
        var bus = Substitute.For<IEventBus>();
        var transport = new ConsoleTransport(bus);

        var action = new OrbAction(
            Guid.NewGuid(), OrbActionType.DeleteMessage,
            "t1", "c1", null,
            new ReplyTarget("console", "t1", "c1"),
            new() { ["messageId"] = "123" }
        );

        var ex = await Record.ExceptionAsync(() => transport.SendAsync(action, CancellationToken.None));
        Assert.Null(ex);
    }
}
