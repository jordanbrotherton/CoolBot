using SQLite;

namespace CoolBot.Modules.Debt;

public class Debt
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [Indexed]
    public ulong DebteeId { get; set; }

    public string Description { get; set; } = "";
    public int Amount { get; set; } = 0;
    public DateTime Time { get; set; } = DateTime.UtcNow;

    public Debt() { }

    public Debt(string description, int amount)
    {
        Description = description;
        Amount = amount;
    }
}
