using DSharpPlus;
using Microsoft.Extensions.DependencyInjection;
using Orb.Core;

namespace Orb.Transport.Discord;

public sealed class DiscordTransportBuilder
{
    private readonly IServiceCollection _services;
    private readonly DiscordTransportOptions _options = new();

    internal DiscordTransportBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public DiscordTransportBuilder Configure(Action<DiscordTransportOptions> configure)
    {
        configure(_options);
        return this;
    }

    public IServiceCollection Register()
    {
        _services.AddSingleton(_options);

        _services.AddSingleton(sp =>
        {
            var opts = sp.GetRequiredService<DiscordTransportOptions>();

            return new DiscordClient(
                new DiscordConfiguration
                {
                    Token = opts.Token,
                    TokenType = TokenType.Bot,
                    Intents = opts.Intents,
                }
            );
        });

        _services.AddSingleton<ITransport, DiscordTransport>();

        return _services;
    }
}
