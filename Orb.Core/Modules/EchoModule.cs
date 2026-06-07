using Orb.Abstractions;

namespace Orb.Core.Modules;

public sealed class EchoModule : CommandModule
{
    public override string Name => "echo";

    public override async Task ExecuteAsync(OrbEvent e, IKernelContext ctx, CancellationToken ct)
    {
        var response = e.Data["commandline"];

        await ctx.EmitAction(
            new OrbAction(
                e.CorrelationId,
                Type: OrbActionType.SendMessage,
                TenantId: e.TenantId,
                ChannelId: e.ChannelId,
                UserId: e.UserId,
                ReplyTo: ctx.Scope.ReplyTo,
                Data: new Dictionary<string, object> { ["content"] = response }
            ),
            ct
        );
    }
}
