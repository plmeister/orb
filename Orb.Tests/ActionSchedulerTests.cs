using NSubstitute;
using Orb.Abstractions;
using Orb.Core;
using Orb.Host;

namespace Orb.Tests;

public class ActionSchedulerTests
{
    [Fact]
    public async Task Schedule_item_becomes_due_is_written_to_channel()
    {
        var channel = new ActionChannel();
        var scheduler = new ActionScheduler(channel);

        var action = new OrbAction(
            Guid.NewGuid(), OrbActionType.SendMessage,
            "t1", "c1", "u1",
            new ReplyTarget("test", "t1", "c1"),
            new() { ["content"] = "hello" }
        );

        scheduler.Schedule(new ScheduledActionItem(
            Guid.NewGuid(), action, DateTimeOffset.UtcNow.AddMilliseconds(-1)
        ));

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        var reader = channel.ReadAllAsync(cts.Token);

        // Start the scheduler background loop
        _ = scheduler.StartAsync(cts.Token);

        var completed = await reader.AnyAsync(cts.Token);
        Assert.True(completed);
    }

    [Fact]
    public async Task Future_item_is_not_executed_before_due_time()
    {
        var channel = new ActionChannel();
        var scheduler = new ActionScheduler(channel);

        var action = new OrbAction(
            Guid.NewGuid(), OrbActionType.SendMessage,
            "t1", "c1", "u1",
            new ReplyTarget("test", "t1", "c1"),
            new() { ["content"] = "later" }
        );

        scheduler.Schedule(new ScheduledActionItem(
            Guid.NewGuid(), action, DateTimeOffset.UtcNow.AddHours(1)
        ));

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
        _ = scheduler.StartAsync(cts.Token);

        try
        {
            var reader = channel.ReadAllAsync(cts.Token);
            await reader.AnyAsync(cts.Token);
            Assert.Fail("Should have timed out without receiving an item");
        }
        catch (OperationCanceledException)
        {
            // Expected — no items were due
        }
    }

    [Fact]
    public void Schedule_is_thread_safe()
    {
        var channel = Substitute.For<IActionChannel>();
        var scheduler = new ActionScheduler(channel);

        var item = new ScheduledActionItem(
            Guid.NewGuid(),
            new OrbAction(
                Guid.NewGuid(), OrbActionType.SendMessage,
                null, null, null,
                new ReplyTarget("t", null, null), []
            ),
            DateTimeOffset.MaxValue
        );

        var ex = Record.Exception(() =>
            Parallel.For(0, 100, _ => scheduler.Schedule(item))
        );

        Assert.Null(ex);
    }
}
