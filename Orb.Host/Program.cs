using Orb.Abstractions;
using Orb.Core;
using Orb.Core.Modules;
using Orb.Host;
using Orb.Storage;
using Orb.Transport.Console;
using Orb.Transport.Discord;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o =>
{
    o.SingleLine = true;
    o.TimestampFormat = "HH:mm:ss ";
    o.IncludeScopes = true;
});

builder.Services.AddSingleton<Kernel>();
builder.Services.AddSingleton<IKernelContextFactory, KernelContextFactory>();
builder.Services.AddSingleton<IKernelEmitter, KernelEmitter>();

builder.Services.AddSingleton<IActionChannel, ActionChannel>();
builder.Services.AddSingleton<IEventBus, EventBus>();
builder.Services.AddSingleton<IEventSink>(sp => sp.GetRequiredService<Kernel>());

builder.Services.AddSingleton<IStorage>(_ => new FileStorage("./data"));

builder.Services.AddSingleton<IActionScheduler, ActionScheduler>();

builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<EventDispatcher>();
builder.Services.AddHostedService<ActionDispatcher>();
builder.Services.AddHostedService(sp => (ActionScheduler)sp.GetRequiredService<IActionScheduler>());

builder.Services.AddSingleton<IModule, CommandParserModule>();
builder.Services.AddSingleton<IModule, PingModule>();
builder.Services.AddSingleton<IModule, EchoModule>();
builder.Services.AddSingleton<IModule, AutoDeleteAttachmentsModule>();

// wire up console transport
builder.Services.AddSingleton<ITransport, ConsoleTransport>();

// wire up discord Transport
builder
    .Services.AddDiscordTransport()
    .Configure(opts =>
    {
        opts.Token =
            builder.Configuration["Discord:Token"]
            ?? throw new InvalidOperationException("Missing Discord token");
    })
    .Register();

var host = builder.Build();
host.Run();
