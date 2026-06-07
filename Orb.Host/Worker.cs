using Orb.Core;

namespace Orb.Host;

public class Worker(IEnumerable<ITransport> transports) : BackgroundService
{
    private readonly IEnumerable<ITransport> _transports = transports;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = new List<Task>();
        foreach (var t in _transports)
        {
            tasks.Add(t.StartAsync(stoppingToken));
        }
        return Task.WhenAll(tasks);
    }
}
