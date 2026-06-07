using Orb.Abstractions;
using Orb.Core;

namespace Orb.Host;

public sealed class ActionScheduler(IActionChannel actions) : BackgroundService, IActionScheduler
{
    private readonly IActionChannel _actions = actions;

    private readonly List<ScheduledActionItem> _scheduled = [];
    private readonly Lock _lock = new();

    public void Schedule(ScheduledActionItem item)
    {
        lock (_lock)
        {
            _scheduled.Add(item);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            List<ScheduledActionItem> due;

            lock (_lock)
            {
                var now = DateTimeOffset.UtcNow;

                due = [.. _scheduled.Where(x => x.ExecuteAt <= now)];

                _scheduled.RemoveAll(x => x.ExecuteAt <= now);
            }

            foreach (var item in due)
            {
                await _actions.WriteAsync(item.Action, stoppingToken);
            }

            await Task.Delay(200, stoppingToken);
        }
    }
}
