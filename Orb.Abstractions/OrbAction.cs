namespace Orb.Abstractions;

public enum OrbActionType
{
    SendMessage,
    DeleteMessage,
}

public sealed record OrbAction(
    Guid CorrelationId,
    OrbActionType Type,
    string? TenantId,
    string? ChannelId,
    string? UserId,
    ReplyTarget ReplyTo,
    Dictionary<string, object> Data
);
