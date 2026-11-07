using SQLite;

namespace CoolBot.Modules.Tally;

public class Tally
{
    [PrimaryKey]
    public string Name { get; set; } = "";
    public int Count { get; set; } = 0;
}
