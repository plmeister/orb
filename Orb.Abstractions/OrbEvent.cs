namespace Orb.Abstractions;

public enum OrbEventType
{
    MessageCreated,
    CommandDetected,
}

public sealed record OrbEvent(
    Guid CorrelationId,
    OrbEventType Type,
    string OriginTransport,
    string? TenantId,
    string? ChannelId,
    string? UserId,
    long Timestamp,
    Dictionary<string, object> Data
);
