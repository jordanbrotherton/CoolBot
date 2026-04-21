using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace CoolBot.Modules;

public class AskManager : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly List<string> _responses;

    public AskManager(IConfiguration config)
    {
        _responses = config.GetSection("Ask:Responses").Get<List<string>>() ?? new List<string>();
    }

    /// <summary>
    /// Allows a user to seek the bot's wisdom of a magic 8 ball.
    /// </summary>
    /// <param name="question">The question the user provides.</param>
    /// <returns>A Discord message of the bot's random "wisdom".</returns>
    [SlashCommand("ask", "Seek the wisdom of CoolBot!")]
    public async Task<InteractionMessageProperties> Ask(
        [SlashCommandParameter(Name = "question", Description = "The question you seek to answer.")] string question
    )
    {
        InteractionMessageProperties output = new()
        {
            Content = $"**<@{Context.User.Id}> asked:** {question}\n**<@{Context.Client.Id}>'s wisdom:** ",
            AllowedMentions = new AllowedMentionsProperties { AllowedUsers = [] }
        };

        if (_responses.Count == 0)
        {
            output.Content += ("It is empty.");
        }
        else
        {
            var idx = (Random.Shared.Next(_responses.Count));
            output.Content += _responses[idx];
        }
        
        return output;
    }
}