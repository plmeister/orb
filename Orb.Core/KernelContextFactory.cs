using Microsoft.Extensions.DependencyInjection;
using Orb.Abstractions;

namespace Orb.Core;

public class KernelContextFactory(IServiceProvider sp) : IKernelContextFactory
{
    private readonly IServiceProvider _sp = sp;

    public IKernelContext Create(OrbExecutionScope scope) =>
        ActivatorUtilities.CreateInstance<KernelContext>(_sp, scope);
}
