using Orb.Abstractions;

namespace Orb.Core.Modules;

public sealed class AutoDeleteAttachmentsModule(IActionScheduler scheduler, IStorage storage)
    : CommandModule
{
    private readonly IActionScheduler _scheduler = scheduler;
    private readonly IStorage _storage = storage;

    public override string Name => "autodelete";

    public override async Task ExecuteAsync(OrbEvent e, IKernelContext ctx, CancellationToken ct)
    {
        var args = (string[])e.Data["args"];
        if (int.TryParse(args[0], out var seconds))
        {
            var key = $"autodelete:{ctx.Scope.ChannelId}";
            if (seconds > 0)
            {
                await _storage.PutAsync(ctx.Scope.TenantId, key, seconds, ct);
            }
            else
            {
                await _storage.DeleteAsync(ctx.Scope.TenantId, key, ct);
            }
        }
    }

    public override async Task<bool> HandleEvent(
        OrbEvent e,
        IKernelContext ctx,
        CancellationToken ct
    )
    {
        var key = $"autodelete:{ctx.Scope.ChannelId}";
        try
        {
            var x = await _storage.GetAsync<int>(ctx.Scope.TenantId!, key, ct);
            if (x <= 0)
                return false;

            var deleteAction = new OrbAction(
                CorrelationId: e.CorrelationId,
                Type: OrbActionType.DeleteMessage,
                TenantId: e.TenantId!,
                ChannelId: e.ChannelId!,
                UserId: null,
                ReplyTo: ctx.Scope.ReplyTo,
                Data: new Dictionary<string, object> { ["messageId"] = e.Data["messageId"] }
            );
            var item = new ScheduledActionItem(
                Guid.NewGuid(),
                deleteAction,
                DateTimeOffset.Now.Add(TimeSpan.FromSeconds(x))
            );
            _scheduler.Schedule(item);

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}
