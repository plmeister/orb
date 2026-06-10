using Orb.Abstractions;

namespace Orb.Core.Modules;

public sealed class AutoDeleteAttachmentsModule(IActionScheduler scheduler)
    : CommandModule
{
  private readonly IActionScheduler _scheduler = scheduler;

  public override string Name => "autodelete";

  public override async Task ExecuteAsync(OrbEvent e, IKernelContext ctx, CancellationToken ct)
  {
    var args = (string[])e.Data["args"];
    if (int.TryParse(args[0], out var seconds))
    {
      var key = $"autodelete:{ctx.Scope.ChannelId}";
      if (seconds > 0)
      {
        await ctx.Storage.PutAsync(ctx.Scope.TenantId, key, seconds, ct);
      }
      else
      {
        await ctx.Storage.DeleteAsync(ctx.Scope.TenantId, key, ct);
      }
    }
  }

  public override async Task<bool> HandleEvent(
      OrbEvent e,
      IKernelContext ctx,
      CancellationToken ct
  )
  {
    try
    {
      var hasattachments = e.Data.TryGetValue("attachments", out var attachments) && attachments is List<Dictionary<string, object>> { Count: > 0 };
      if (!hasattachments)
      {
        return false;
      }

      var key = $"autodelete:{ctx.Scope.ChannelId}";
      var (ok, deleteSeconds) = await ctx.Storage.TryGetAsync<int>(ctx.Scope.TenantId!, key, ct);
      if (!ok || deleteSeconds <= 0)
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
          DateTimeOffset.Now.Add(TimeSpan.FromSeconds(deleteSeconds))
      );
      _scheduler.Schedule(item);

      return true;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"exception: {ex}");
    }

    return false;
  }
}
