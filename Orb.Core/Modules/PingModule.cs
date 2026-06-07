using Orb.Abstractions;

namespace Orb.Core.Modules;

public sealed class PingModule : CommandModule
{
    public override string Name => "ping";

    public override async Task ExecuteAsync(OrbEvent e, IKernelContext ctx, CancellationToken ct)
    {
        await ctx.Storage.PutAsync(e.TenantId, "ping", "true", ct);
        await ctx.EmitAction(
            new OrbAction(
                CorrelationId: e.CorrelationId,
                Type: OrbActionType.SendMessage,
                TenantId: e.TenantId,
                ChannelId: e.ChannelId,
                UserId: null,
                ReplyTo: ctx.Scope.ReplyTo,
                Data: new Dictionary<string, object> { ["content"] = "pong" }
            ),
            ct
        );
    }
}
