using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using System.Text;
using SQLite;

namespace CoolBot.Modules.Debt;

public class DebtManager : ApplicationCommandModule<ApplicationCommandContext>
{
    static SQLiteConnection _sql;
    public DebtManager()
    {
        if (_sql == null)
        {
            _sql = new SQLiteConnection("CoolBot.db");
            _sql.CreateTable<Debt>();
        }
    }

    /// <summary>
    /// Adds a debt to a specified user.
    /// </summary>
    /// <param name="user">The provided Discord user to indebt.</param>
    /// <param name="amount">The amount of money owed for the debt.</param>
    /// <param name="description">The reason for the debt.</param>
    /// <returns>A message indicating the result.</returns>
    [SlashCommand("fine", "Add a new debt to a user.")]
    public InteractionMessageProperties AddDebt(
        [SlashCommandParameter(Name = "user", Description = "The user who owes the debt.")] User user,
        [SlashCommandParameter(Name = "amount", Description = "The amount owed.")] int amount,
        [SlashCommandParameter(Name = "description", Description = "Why is this owed?")] string description)
    {
        if (user.Id == Context.Client.Id)
        {
            user = Context.User;
            description = $"What goes around comes around.";
        }

        var debt = new Debt(description, amount)
        {
            DebteeId = user.Id
        };
        _sql.Insert(debt);

        return new InteractionMessageProperties()
        {
            Content = $"Recorded a fine of **{amount}** for <@{user.Id}>. Reason: *{description}*",
            AllowedMentions = new AllowedMentionsProperties { AllowedUsers = [] }
        };
    }

    /// <summary>
    /// Gets all of the debts of a user.
    /// </summary>
    /// <param name="user">The user to get all of the debts for.</param>
    /// <returns>A message with a list of all the debts the user has.</returns>
    [SlashCommand("getdebts", "Get all debts for a user.")]
    public InteractionMessageProperties GetDebts(
        [SlashCommandParameter(Name = "user", Description = "The user who owes the debt.")] User user)
    {
        if (user.Id == Context.Client.Id)
        {
            return new InteractionMessageProperties()
            {
                Content = $"CoolBot is perfect and has **0** debt."
            };
        }

        var debts = _sql.Table<Debt>().Where(x => x.DebteeId == user.Id).ToList();

        if (debts.Count == 0)
        {
            return new InteractionMessageProperties()
            {
                Content = $"<@{user.Id}> has been good and has **0** debt!",
                AllowedMentions = new AllowedMentionsProperties { AllowedUsers = [] }
            };
        }

        int total = debts.Sum(x => x.Amount);
        var sb = new StringBuilder();
        sb.AppendLine($"**Debts for <@{user.Id}>** (Total: {total})");

        foreach (var debt in debts.OrderByDescending(x => x.Time))
        {
            sb.AppendLine($"**Debt #{debt.Id}** | **{debt.Amount}** | {debt.Description} (<t:{((DateTimeOffset)debt.Time).ToUnixTimeSeconds()}:R>)");
        }

        return new InteractionMessageProperties()
        {
            Content = sb.ToString(),
            AllowedMentions = new AllowedMentionsProperties { AllowedUsers = [] }
        };
    }

    /// <summary>
    /// Allows a user to 'pay off' or remove their debt.
    /// </summary>
    /// <param name="debtId">The debt ID to pay off.</param>
    /// <returns>A message indicating the payoff status.</returns>
    [SlashCommand("pay", "Pay off a debt.")]
    public InteractionMessageProperties Pay(
        [SlashCommandParameter(Name = "debt", Description = "The debt to pay.")] int debtId)
    {
        var debt = _sql.Table<Debt>().FirstOrDefault(x => x.Id == debtId);
        if (debt == null)
        {
            return new InteractionMessageProperties()
            {
                Content = "This debt does not exist. Enter a valid debt ID. Use `/getdebts` to get a list of your debts.",
                Flags = MessageFlags.Ephemeral
            };
        }
        else if (debt.DebteeId != Context.User.Id)
        {
            return new InteractionMessageProperties()
            {
                Content = "This isn't your debt! Use `/getdebts` to get a list of your debts.",
                Flags = MessageFlags.Ephemeral
            };
        }
        else
        {
            _sql.Delete(debt);
            return new InteractionMessageProperties()
            {
                Content = $"<@{Context.User.Id}> paid off debt #{debtId}!",
                AllowedMentions = new AllowedMentionsProperties { AllowedUsers = [] }
            };
        }
    }
}
