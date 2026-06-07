using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Orb.Transport.Discord;

public interface IDiscordGateway
{
    event Func<DiscordClient, MessageCreateEventArgs, Task> MessageCreated;
    Task ConnectAsync();
    Task DisconnectAsync();
    Task<DiscordChannel> GetChannelAsync(ulong id);
}
