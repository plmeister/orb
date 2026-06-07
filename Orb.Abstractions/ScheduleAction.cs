namespace Orb.Abstractions;

public sealed record ScheduleAction(OrbAction Action, DateTimeOffset ExecuteAt);
