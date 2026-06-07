using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Orb.Transport.Discord;

public class DiscordGateway : IDiscordGateway
{
    public event Func<DiscordClient, MessageCreateEventArgs, Task>? MessageCreated;

    public Task ConnectAsync()
    {
        throw new NotImplementedException();
    }

    public Task DisconnectAsync()
    {
        throw new NotImplementedException();
    }

    public Task<DiscordChannel> GetChannelAsync(ulong id)
    {
        throw new NotImplementedException();
    }
}
