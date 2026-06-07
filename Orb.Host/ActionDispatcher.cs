using Orb.Core;

namespace Orb.Host;

public sealed class ActionDispatcher(
    IActionChannel actions,
    IEnumerable<ITransport> transports,
    ILogger<ActionDispatcher> logger
) : BackgroundService
{
    private readonly IActionChannel _actions = actions;
    private readonly IEnumerable<ITransport> _transports = transports;
    private readonly ILogger<ActionDispatcher> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var action in _actions.ReadAllAsync(stoppingToken))
        {
            try
            {
                var transport =
                    _transports.FirstOrDefault(t => t.Name == action.ReplyTo.Transport)
                    ?? throw new InvalidOperationException(
                        $"No transport: {action.ReplyTo.Transport}"
                    );
                await transport.SendAsync(action, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed executing action {ActionType}", action.Type);
            }
        }
    }
}
