using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Orb.Abstractions;
using Orb.Core;

namespace Orb.Transport.Discord;

public sealed class DiscordTransport(DiscordClient client, IEventBus bus) : ITransport
{
    public string Name => "discord";

    private readonly DiscordClient _client = client;
    private readonly IEventBus _bus = bus;

    public Task StartAsync(CancellationToken ct)
    {
        _client.MessageCreated += OnMessageCreated;

        return _client.ConnectAsync();
    }

    public async Task StopAsync(CancellationToken ct)
    {
        _client.MessageCreated -= OnMessageCreated;
        await _client.DisconnectAsync();
    }

    private async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        // ignore bots unless explicitly needed
        if (e.Author.IsBot)
            return;

        var inbound = new OrbEvent(
            CorrelationId: Guid.NewGuid(),
            Type: OrbEventType.MessageCreated,
            OriginTransport: "discord",
            TenantId: e.Guild?.Id.ToString(),
            ChannelId: e.Channel.Id.ToString(),
            UserId: e.Author.Id.ToString(),
            Timestamp: e.Message.Timestamp.ToUnixTimeMilliseconds(),
            Data: new Dictionary<string, object>
            {
                ["messageId"] = e.Message.Id.ToString(),
                ["content"] = e.Message.Content,
                ["attachments"] = MapAttachments(e.Message.Attachments),
            }
        );
        await _bus.PublishAsync(inbound, CancellationToken.None);
    }

    private static List<Dictionary<string, object>> MapAttachments(
        IReadOnlyList<DiscordAttachment> attachments
    )
    {
        return
        [
            .. attachments.Select(a => new Dictionary<string, object>
            {
                ["url"] = a.Url,
                ["filename"] = a.FileName,
                ["size"] = a.FileSize,
            }),
        ];
    }

    public async Task SendAsync(OrbAction action, CancellationToken ct)
    {
        switch (action.Type)
        {
            case OrbActionType.SendMessage:
                await SendMessage(action, ct);
                break;
            case OrbActionType.DeleteMessage:
                await DeleteMessage(action, ct);
                break;
        }
    }

    private async Task SendMessage(OrbAction action, CancellationToken ct)
    {
        var channelId = ulong.Parse(action.ChannelId!);
        var channel = await _client.GetChannelAsync(channelId);

        await channel.SendMessageAsync((string)action.Data["content"]);
    }

    private async Task DeleteMessage(OrbAction action, CancellationToken ct)
    {
        if (!ulong.TryParse(action.ChannelId, out var channelId))
            return;

        if (!ulong.TryParse((string)action.Data["messageId"], out var messageId))
            return;
        var channel = await _client.GetChannelAsync(channelId);
        var message = await channel.GetMessageAsync(messageId);
        await channel.DeleteMessageAsync(message);
    }
}
