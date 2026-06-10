using NSubstitute;
using Orb.Abstractions;
using Orb.Core.Modules;

namespace Orb.Tests;

public class CommandParserModuleTests
{
    private readonly CommandParserModule _sut = new();
    private readonly IKernelContext _ctx = Substitute.For<IKernelContext>();

    [Fact]
    public async Task Parses_bang_command_and_emits_CommandDetected()
    {
        var e = MakeMessage("!ping");

        await _sut.OnEventAsync(e, _ctx, CancellationToken.None);

        await _ctx.Received(1).EmitEvent(
            Arg.Is<OrbEvent>(x =>
                x.Type == OrbEventType.CommandDetected
                && (string)x.Data["command"] == "ping"
            ),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Parses_command_with_args()
    {
        var e = MakeMessage("!echo hello world");

        await _sut.OnEventAsync(e, _ctx, CancellationToken.None);

        await _ctx.Received(1).EmitEvent(
            Arg.Is<OrbEvent>(x =>
                (string)x.Data["command"] == "echo"
                && ((string[])x.Data["args"]).Length == 2
                && ((string[])x.Data["args"])[0] == "hello"
                && ((string[])x.Data["args"])[1] == "world"
            ),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Ignores_message_without_bang_prefix()
    {
        var e = MakeMessage("hello");

        await _sut.OnEventAsync(e, _ctx, CancellationToken.None);

        await _ctx.DidNotReceive().EmitEvent(Arg.Any<OrbEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Ignores_non_MessageCreated_event()
    {
        var e = new OrbEvent(
            CorrelationId: Guid.NewGuid(),
            Type: OrbEventType.CommandDetected,
            OriginTransport: "test",
            TenantId: null,
            ChannelId: null,
            UserId: null,
            Timestamp: 0,
            Data: []
        );

        await _sut.OnEventAsync(e, _ctx, CancellationToken.None);

        await _ctx.DidNotReceive().EmitEvent(Arg.Any<OrbEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Ignores_message_without_content_key()
    {
        var e = new OrbEvent(
            CorrelationId: Guid.NewGuid(),
            Type: OrbEventType.MessageCreated,
            OriginTransport: "test",
            TenantId: null,
            ChannelId: null,
            UserId: null,
            Timestamp: 0,
            Data: []
        );

        await _sut.OnEventAsync(e, _ctx, CancellationToken.None);

        await _ctx.DidNotReceive().EmitEvent(Arg.Any<OrbEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Ignores_whitespace_only_message()
    {
        var e = MakeMessage("   ");

        await _sut.OnEventAsync(e, _ctx, CancellationToken.None);

        await _ctx.DidNotReceive().EmitEvent(Arg.Any<OrbEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Converts_command_to_lowercase()
    {
        var e = MakeMessage("!PING");

        await _sut.OnEventAsync(e, _ctx, CancellationToken.None);

        await _ctx.Received(1).EmitEvent(
            Arg.Is<OrbEvent>(x => (string)x.Data["command"] == "ping"),
            Arg.Any<CancellationToken>()
        );
    }

    private static OrbEvent MakeMessage(string content) => new(
        CorrelationId: Guid.NewGuid(),
        Type: OrbEventType.MessageCreated,
        OriginTransport: "test",
        TenantId: "t1",
        ChannelId: "c1",
        UserId: "u1",
        Timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        Data: new() { ["content"] = content, ["messageId"] = "m1" }
    );
}
