using Microsoft.Extensions.Configuration;
using NetCord;
using NetCord.Rest;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace CoolBot.Modules;

public class ForumManager : IGuildThreadCreateGatewayHandler
{
    private readonly Dictionary<ulong, ulong> _threadToChannelMap;
    private readonly GatewayClient _client;

    public ForumManager(IConfiguration config, GatewayClient client)
    {
        _threadToChannelMap = config.GetSection("Forum:Map").Get<Dictionary<ulong, ulong>>() ?? new Dictionary<ulong, ulong>();
        _client = client;
    }

    public async ValueTask HandleAsync(GuildThreadCreateEventArgs args)
    {
        if (args.Thread.ParentId is ulong parentID)
        {
            if (_threadToChannelMap.TryGetValue(parentID, out ulong channelID))
            {
                MessageProperties message = new()
                {
                    Content = $"@everyone\n# New event plan!\n*Be sure to follow if you want to stay updated!*",

                    Embeds = [
                            new EmbedProperties
                            {
                                Title = args.Thread.Name,
                                Description = $"**Created by:** <@{args.Thread.OwnerId}>",
                                Color = new Color(0x5865F2),
                                Timestamp = DateTimeOffset.UtcNow
                            }
                    ],

                    Components = [
                        new ActionRowProperties{
                            new LinkButtonProperties($"https://discord.com/channels/{args.Thread.GuildId}/{args.Thread.Id}", "Go to Thread")
                        }
                    ]
                };
                await _client.Rest.SendMessageAsync(channelID, message);
            }
        }
    }
}
