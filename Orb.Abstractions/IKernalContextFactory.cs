namespace Orb.Abstractions;

public interface IKernelContextFactory
{
    IKernelContext Create(OrbExecutionScope scope);
}
