using TShockAPI;

namespace RoleSelection;

public class RoleData : PlayerData
{
    public string Role { get; set; } = string.Empty;
    public int Account { get; set; }

    public RoleData(TSPlayer plr, string role) : base(plr)
    {
        Role = role;
        Account = plr.Account.ID;
    }

    public RoleData(TSPlayer plr) : base(plr)
    {
        Account = plr.Account.ID;
    }

    public RoleData() : base(new TSPlayer(-1))
    {
        Account = -1;
    }
}
