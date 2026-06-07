namespace Orb.Abstractions;

public sealed record ScheduledActionItem(Guid Id, OrbAction Action, DateTimeOffset ExecuteAt);

public interface IActionScheduler
{
    void Schedule(ScheduledActionItem item);
}
