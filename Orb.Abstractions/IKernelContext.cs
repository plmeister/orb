namespace Orb.Abstractions;

public interface IKernelContext
{
    OrbExecutionScope Scope { get; }
    Task EmitAction(OrbAction a, CancellationToken ct);
    Task EmitEvent(OrbEvent e, CancellationToken ct);
    IStorage Storage { get; }
}
