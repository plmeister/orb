namespace Orb.Abstractions;

public sealed record OrbExecutionScope(
    Guid CorrelationId,
    string? TenantId,
    string? ChannelId,
    string? UserId,
    ReplyTarget ReplyTo
);
