using Microsoft.Extensions.Logging;
using Orb.Abstractions;

namespace Orb.Core;

public sealed class Kernel : IEventSink
{
    private readonly List<IModule> _modules = [];
    private readonly ILogger<Kernel> _logger;
    private readonly IKernelContextFactory _factory;
    private readonly IStorage _storage;

    public Kernel(
        IEnumerable<IModule> modules,
        ILogger<Kernel> logger,
        IKernelContextFactory factory,
        IStorage storage
    )
    {
        _factory = factory;
        _logger = logger;
        _storage = storage;
        foreach (var m in modules)
        {
            Register(m);
        }
    }

    public void Register(IModule module)
    {
        _modules.Add(module);
    }

    public async Task PublishAsync(OrbEvent e, CancellationToken ct)
    {
        var scope = new OrbExecutionScope(
            CorrelationId: e.CorrelationId,
            TenantId: e.TenantId,
            ChannelId: e.ChannelId,
            UserId: e.UserId,
            ReplyTo: new ReplyTarget(
                Transport: e.OriginTransport,
                TenantId: e.TenantId,
                ChannelId: e.ChannelId,
                UserId: e.UserId
            )
        );

        using var _ = _logger.BeginScope(
            new
            {
                scope.CorrelationId,
                scope.TenantId,
                scope.ChannelId,
                scope.UserId,
            }
        );

        _logger.LogInformation("Handling event {EventType}", e.Type);

        var ctx = _factory.Create(scope);

        var tasks = new List<Task>();

        foreach (var module in _modules)
        {
            _logger.LogDebug("Dispatching module {Module}", module.Name);
            tasks.Add(module.OnEventAsync(e, ctx, ct));
        }

        await Task.WhenAll(tasks);
    }
}
