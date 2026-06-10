using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Orb.Abstractions;
using Orb.Core;

namespace Orb.Tests;

public class KernelTests
{
    [Fact]
    public async Task Dispatches_event_to_all_registered_modules()
    {
        var m1 = Substitute.For<IModule>();
        m1.Name.Returns("m1");
        var m2 = Substitute.For<IModule>();
        m2.Name.Returns("m2");

        var factory = Substitute.For<IKernelContextFactory>();
        factory.Create(Arg.Any<OrbExecutionScope>()).Returns(Substitute.For<IKernelContext>());

        var storage = Substitute.For<IStorage>();

        var kernel = new Kernel([m1, m2], NullLogger<Kernel>.Instance, factory, storage);

        var e = new OrbEvent(
            Guid.NewGuid(), OrbEventType.MessageCreated,
            "test", "t1", "c1", "u1",
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            []
        );

        await kernel.PublishAsync(e, CancellationToken.None);

        await m1.Received(1).OnEventAsync(Arg.Is(e), Arg.Any<IKernelContext>(), Arg.Any<CancellationToken>());
        await m2.Received(1).OnEventAsync(Arg.Is(e), Arg.Any<IKernelContext>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Creates_scope_with_correct_reply_target()
    {
        var module = Substitute.For<IModule>();
        module.Name.Returns("m");

        IKernelContext? capturedCtx = null;
        var factory = Substitute.For<IKernelContextFactory>();
        factory.Create(Arg.Any<OrbExecutionScope>()).Returns(c =>
        {
            capturedCtx = Substitute.For<IKernelContext>();
            capturedCtx.Scope.Returns(c.Arg<OrbExecutionScope>());
            return capturedCtx;
        });

        var storage = Substitute.For<IStorage>();
        var kernel = new Kernel([module], NullLogger<Kernel>.Instance, factory, storage);

        var e = new OrbEvent(
            Guid.NewGuid(), OrbEventType.MessageCreated,
            "console", "t1", "c1", "u1",
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            []
        );

        await kernel.PublishAsync(e, CancellationToken.None);

        Assert.NotNull(capturedCtx);
        Assert.Equal("console", capturedCtx.Scope.ReplyTo.Transport);
        Assert.Equal("t1", capturedCtx.Scope.ReplyTo.TenantId);
        Assert.Equal("c1", capturedCtx.Scope.ReplyTo.ChannelId);
        Assert.Equal("u1", capturedCtx.Scope.ReplyTo.UserId);
    }

    [Fact]
    public async Task Handles_no_modules_gracefully()
    {
        var factory = Substitute.For<IKernelContextFactory>();
        factory.Create(Arg.Any<OrbExecutionScope>()).Returns(Substitute.For<IKernelContext>());

        var kernel = new Kernel([], NullLogger<Kernel>.Instance, factory, Substitute.For<IStorage>());

        var e = new OrbEvent(
            Guid.NewGuid(), OrbEventType.MessageCreated,
            "test", null, null, null, 0, []
        );

        var ex = await Record.ExceptionAsync(() => kernel.PublishAsync(e, CancellationToken.None));
        Assert.Null(ex);
    }
}
