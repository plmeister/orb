using Microsoft.Extensions.DependencyInjection;

namespace Orb.Transport.Discord;

public static class DiscordTransportExtensions
{
    public static DiscordTransportBuilder AddDiscordTransport(this IServiceCollection services)
    {
        return new DiscordTransportBuilder(services);
    }
}
