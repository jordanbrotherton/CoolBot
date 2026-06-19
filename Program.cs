using Microsoft.Extensions.Hosting;
using NetCord;

using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

using NetCord.Gateway;
using CoolBot.Modules;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddDiscordGateway(options =>
    {
        options.Presence = new PresenceProperties(UserStatusType.Online)
        {
            Activities = [new UserActivityProperties("Tracking your debts with a close eye...", UserActivityType.Watching)]
        };
    })
    .AddApplicationCommands().AddGatewayHandler<ForumManager>();

var host = builder.Build();

host.AddModules(typeof(Program).Assembly);

await host.RunAsync();
