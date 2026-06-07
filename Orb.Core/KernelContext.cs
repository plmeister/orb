using Microsoft.Extensions.Logging;
using Orb.Abstractions;

namespace Orb.Core;

public sealed class KernelContext(
    OrbExecutionScope scope,
    ILogger<KernelContext> logger,
    IKernelEmitter emitter,
    IStorage storage
) : IKernelContext
{
    private readonly OrbExecutionScope _scope = scope;
    private readonly ILogger<KernelContext> _logger = logger;
    private readonly IKernelEmitter _emitter = emitter;
    private readonly IStorage _storage = storage;

    public IStorage Storage => _storage;

    public OrbExecutionScope Scope => _scope;

    public Task EmitAction(OrbAction a, CancellationToken ct) => _emitter.EmitActionAsync(a, ct);

    public Task EmitEvent(OrbEvent e, CancellationToken ct) => _emitter.EmitEventAsync(e, ct);
}
