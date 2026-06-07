using Orb.Abstractions;

namespace Orb.Core.Modules;

public sealed class CommandParserModule : IModule
{
    public string Name => "command-parser";

    public async Task OnEventAsync(OrbEvent e, IKernelContext ctx, CancellationToken ct)
    {
        if (e.Type != OrbEventType.MessageCreated)
            return;

        if (!e.Data.TryGetValue("content", out var rawObj))
            return;

        var raw = rawObj?.ToString();
        if (string.IsNullOrWhiteSpace(raw))
            return;

        raw = raw.Trim();

        if (!raw.StartsWith('!'))
            return;

        var commandLine = raw[1..];

        var parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return;

        var commandName = parts[0].ToLowerInvariant();
        var args = parts.Skip(1).ToArray();

        var cmdEvent = new OrbEvent(
            CorrelationId: Guid.NewGuid(),
            Type: OrbEventType.CommandDetected,
            OriginTransport: e.OriginTransport,
            TenantId: e.TenantId,
            ChannelId: e.ChannelId,
            UserId: e.UserId,
            Timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Data: new Dictionary<string, object>
            {
                ["command"] = commandName,
                ["args"] = args,
                ["commandline"] = string.Join(" ", args),
                ["sourceEventId"] = e.CorrelationId,
                ["messageId"] = e.Data["messageId"],
            }
        );

        await ctx.EmitEvent(cmdEvent, ct);
    }
}
