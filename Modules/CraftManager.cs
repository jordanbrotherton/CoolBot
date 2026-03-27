using CoreRCON;
using Microsoft.Extensions.Configuration;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using System.Diagnostics;
using System.Net;

namespace CoolBot.Modules;

public class CraftManager : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly IConfiguration _config;
    private readonly bool _isEnabled;

    public CraftManager(IConfiguration config)
    {
        _config = config;
        _isEnabled = !string.IsNullOrEmpty(_config["Minecraft:ip"]);
    }

    /// <summary>
    /// Allows a user to add their username to a Minecraft server's whitelist.
    /// </summary>
    /// <param name="minecraftUsername">The username the user wants to add.</param>
    /// <returns>A Discord message of whether it was successful or not.</returns>
    [SlashCommand("join", "Add your username to the whitelist!")]
    public async Task<InteractionMessageProperties> Join(
        [SlashCommandParameter(Name = "username", Description = "Your Minecraft username.")] string minecraftUsername
    )
    {
        if (_isEnabled)
        {
            var ip = _config["Minecraft:ip"] ?? throw new InvalidOperationException("Minecraft:ip not configured");
            var port = int.Parse(_config["Minecraft:port"] ?? throw new InvalidOperationException("Minecraft:port not configured"));
            var password = _config["Minecraft:password"] ?? throw new InvalidOperationException("Minecraft:password not configured");
            var endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
            
            var rcon = new RCON(endpoint, password);
            await rcon.ConnectAsync();

            bool authorized = await rcon.AuthenticateAsync();

            if (authorized)
            {
                await rcon.SendCommandAsync($"whitelist add {minecraftUsername}");
                return new InteractionMessageProperties()
                {
                    Content = $"Success!",
                    Flags = MessageFlags.Ephemeral
                };
            }
            else
            {
                return new InteractionMessageProperties()
                {
                    Content = $"Could not add user... (Bot exception)",
                    Flags = MessageFlags.Ephemeral
                };
            }
        }
        else
        {
            return new InteractionMessageProperties()
            {
                Content = $"This instance of CoolBot does not have a Minecraft server associated with it.",
                Flags = MessageFlags.Ephemeral
            };
        }
    }
}
