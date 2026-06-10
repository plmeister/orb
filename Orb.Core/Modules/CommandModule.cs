using Orb.Abstractions;

namespace Orb.Core.Modules;

public abstract class CommandModule : IModule
{
    public virtual string Name => "";

    public async Task OnEventAsync(OrbEvent e, IKernelContext ctx, CancellationToken ct)
    {
        if (e.Type == OrbEventType.CommandDetected)
        {
            if ((string)e.Data["command"] != Name)
                return;

            await ExecuteAsync(e, ctx, ct);
        }
        else
        {
            _ = SafeHandleEvent(e, ctx, ct);
        }
    }

    public virtual async Task<bool> HandleEvent(
        OrbEvent e,
        IKernelContext ctx,
        CancellationToken ct
    ) => false;

    public virtual async Task ExecuteAsync(OrbEvent e, IKernelContext ctx, CancellationToken ct) { }

    private async Task SafeHandleEvent(OrbEvent e, IKernelContext ctx, CancellationToken ct)
    {
        try
        {
            await HandleEvent(e, ctx, ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"HandleEvent failed: {ex}");
        }
    }
}
