namespace Orb.Abstractions;

public interface IModule
{
    string Name { get; }
    Task OnEventAsync(OrbEvent e, IKernelContext ctx, CancellationToken ct);
}
