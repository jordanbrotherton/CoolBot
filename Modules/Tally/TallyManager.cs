using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using SQLite;

namespace CoolBot.Modules.Tally;

public class TallyManager : ApplicationCommandModule<ApplicationCommandContext>
{
    static SQLiteConnection? _sql;

    public TallyManager()
    {
        if (_sql == null)
        {
            _sql = new SQLiteConnection("CoolBot.db");
            _sql.CreateTable<Tally>();
        }
    }

    [SlashCommand("tally", "Increment or initialize a tally counter.")]
    public InteractionMessageProperties Bump(
        [SlashCommandParameter(Name = "name", Description = "Name of the tally to bump.")] string name)
    {
        string normalizedKey = name.Trim().ToLowerInvariant();

        var record = _sql!.Table<Tally>().FirstOrDefault(x => x.Name == normalizedKey);
        if (record == null)
        {
            record = new Tally { Name = normalizedKey, Count = 1 };
            _sql.Insert(record);
        }
        else
        {
            record.Count++;
            _sql.Update(record);
        }

        return new InteractionMessageProperties
        {
            Content = $"**<@{Context.User.Id}> added to:** `{name}`\n**New count:** **{record.Count}**",
            AllowedMentions = new AllowedMentionsProperties { AllowedUsers = [] }
        };
    }

    [SlashCommand("gettally", "Check the current value of a tally.")]
    public InteractionMessageProperties Check(
        [SlashCommandParameter(Name = "name", Description = "Name of the tally to check.")] string name)
    {
        string normalizedKey = name.Trim().ToLowerInvariant();
        var record = _sql!.Table<Tally>().FirstOrDefault(x => x.Name == normalizedKey);

        int count = record?.Count ?? 0;

        return new InteractionMessageProperties
        {
            Content = $"**Tally for** `{name}`: **{count}**",
            AllowedMentions = new AllowedMentionsProperties { AllowedUsers = [] }
        };
    }
}
