using DSharpPlus;

namespace Orb.Transport.Discord;

public sealed record DiscordTransportOptions
{
    public string Token { get; set; } = "";

    public DiscordIntents Intents { get; init; } =
        DiscordIntents.GuildMessages
        | DiscordIntents.DirectMessages
        | DiscordIntents.MessageContents;
}
