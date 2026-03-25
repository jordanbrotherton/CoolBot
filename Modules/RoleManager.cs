using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using System.Diagnostics;

namespace CoolBot.Modules;

public class RoleManager : ApplicationCommandModule<ApplicationCommandContext>
{
    /// <summary>
    /// Allows a user to change their username's color via a created/assigned role.
    /// </summary>
    /// <param name="color">The hex color code the user provides.</param>
    /// <returns>A Discord message of whether it was successful or not.</returns>
    [SlashCommand("role", "Get a role color!")]
    public async Task<InteractionMessageProperties> Role(
        [SlashCommandParameter(Name = "color", Description = "The hex color code for your role.")] string color
    )
    {
        InteractionMessageProperties output = new()
        {
            Content = "",
            Flags = MessageFlags.Ephemeral
        };

        var user = Context.User;

        if (Context.Guild == null)
        {
            output.Content = "Command can only be ran in a server.";
            return output;
        }

        if (!Context.Guild.Users.TryGetValue(user.Id, out var guildUser))
            guildUser = await Context.Client.Rest.GetGuildUserAsync(Context.Guild.Id, user.Id);
        var guildRoles = Context.Guild.Roles.Values;

        try
        {
            color = color.Replace("#", "");
            if (!int.TryParse(color, System.Globalization.NumberStyles.HexNumber, null, out var colorValue))
            {
                output.Content = "Invalid color code! Make sure it is in hex format!";
                return output;
            }
            foreach (var role in guildRoles)
            {
                if (role.Permissions == 0)
                {
                    if (guildUser.RoleIds.Contains(role.Id))
                        await guildUser.RemoveRoleAsync(role.Id);

                    var c = role.Colors.PrimaryColor.GetHashCode();
                    if (c == colorValue)
                    {
                        await guildUser.AddRoleAsync(role.Id);
                        output.Content = $"Existing color assigned! ({color})";
                        return output;
                    }
                }
            }

            var roleProperties = new RoleProperties()
            {
                Name = $"Color {color}",
                Colors = new RoleColorsProperties(new Color(colorValue)),
                Permissions = 0
            };
            var newRole = await Context.Guild.CreateRoleAsync(roleProperties);
            await guildUser.AddRoleAsync(newRole.Id);

            output.Content = $"Assigned new color! ({color})";
            return output;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            output.Content = "Could not set color... (Bot exception)";
            return output;
        }
    }
}
