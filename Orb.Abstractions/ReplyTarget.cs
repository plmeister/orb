namespace Orb.Abstractions;

public sealed record ReplyTarget(
    string Transport, // "console", "discord", "http", etc.
    string? TenantId,
    string? ChannelId, // logical channel in that transport
    string? UserId = null // optional direct reply target
);
