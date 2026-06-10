using Orb.Abstractions;
using Orb.Core;

namespace Orb.Transport.Console;

public sealed class ConsoleTransport(IEventBus bus) : ITransport
{
    public string Name => "console";

    public Task StartAsync(CancellationToken ct)
    {
        return Task.Factory.StartNew(
            () => ReadLoop(ct),
            ct,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default
        );
    }

    private async Task ReadLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var input = System.Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                continue;

            var e = new OrbEvent(
                CorrelationId: Guid.NewGuid(),
                Type: OrbEventType.MessageCreated,
                OriginTransport: "console",
                TenantId: "dev",
                ChannelId: "default",
                UserId: "local",
                Timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Data: new Dictionary<string, object> { ["content"] = input, ["messageId"] = "0" }
            );

            try
            {
                await bus.PublishAsync(e, ct);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Publish failed: {ex}");
            }
        }
    }

    public Task SendAsync(OrbAction action, CancellationToken ct)
    {
        var content = action.Data.TryGetValue("content", out var c) ? c : "(no content)";
        System.Console.WriteLine($"[ACTION] {action.Type} {content}");
        return Task.CompletedTask;
    }
}
