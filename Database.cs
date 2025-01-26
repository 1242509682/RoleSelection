using Microsoft.Xna.Framework;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using TShockAPI;
using TShockAPI.DB;

namespace RoleSelection;

public class MyPlayerData
{
    public string Name { get; set; } = ""; // 玩家名字
    public string Role { get; set; } = "无"; // 角色名称
    public bool Cooldown { get; set; } = false; // 冷却键
    public DateTime CoolTime { get; set; } = DateTime.Now; // 冷却时间
    public Dictionary<int, int> Buff { get; set; } = new Dictionary<int, int>();
    public MyPlayerData(string name = "", string role = "无", bool cooldown = false, DateTime coolTime = default, Dictionary<int, int> buff = null!)
    {
        this.Name = name ?? "";
        this.Role = role ?? "无";
        this.Cooldown = cooldown;
        this.CoolTime = coolTime;
        this.Buff = buff ?? new Dictionary<int, int>();
    }
}

public class Database
{
    #region 数据库表结构（使用Tshock自带的数据库作为存储）
    private readonly string RolePlayer;
    private readonly string RoleData;
    public Database()
    {
        RolePlayer = "RolePlayer";
        var sql = new SqlTableCreator(TShock.DB, new SqliteQueryCreator());
        sql.EnsureTableStructure(new SqlTable(RolePlayer, //表名
            new SqlColumn("Name", MySqlDbType.TinyText)
            {
                NotNull = true,
                Primary = true,
                Unique = true
            },
            new SqlColumn("Role", MySqlDbType.TinyText) { NotNull = true },
            new SqlColumn("Cooldown", MySqlDbType.Int32) { DefaultValue = "0" },
            new SqlColumn("CoolTime", MySqlDbType.Text) { DefaultValue = "CURRENT_TIMESTAMP" },
            new SqlColumn("Buff", MySqlDbType.Text)
        ));

        RoleData = "RoleData";
        sql.EnsureTableStructure(new SqlTable(RoleData,
            new SqlColumn("AccAndSlot", MySqlDbType.Text)
            {
                Primary = true
            },
            new SqlColumn("Role", MySqlDbType.TinyText) { NotNull = true },
            new SqlColumn("Account", MySqlDbType.Int32),
            new SqlColumn("Name", MySqlDbType.Text),
            new SqlColumn("Health", MySqlDbType.Int32),
            new SqlColumn("MaxHealth", MySqlDbType.Int32),
            new SqlColumn("Mana", MySqlDbType.Int32),
            new SqlColumn("MaxMana", MySqlDbType.Int32),
            new SqlColumn("Inventory", MySqlDbType.Text),
            new SqlColumn("extraSlot", MySqlDbType.Int32),
            new SqlColumn("spawnX", MySqlDbType.Int32),
            new SqlColumn("spawnY", MySqlDbType.Int32),
            new SqlColumn("skinVariant", MySqlDbType.Int32),
            new SqlColumn("hair", MySqlDbType.Int32),
            new SqlColumn("hairDye", MySqlDbType.Int32),
            new SqlColumn("hairColor", MySqlDbType.Int32),
            new SqlColumn("pantsColor", MySqlDbType.Int32),
            new SqlColumn("shirtColor", MySqlDbType.Int32),
            new SqlColumn("underShirtColor", MySqlDbType.Int32),
            new SqlColumn("shoeColor", MySqlDbType.Int32),
            new SqlColumn("hideVisuals", MySqlDbType.Int32),
            new SqlColumn("skinColor", MySqlDbType.Int32),
            new SqlColumn("eyeColor", MySqlDbType.Int32),
            new SqlColumn("questsCompleted", MySqlDbType.Int32),
            new SqlColumn("usingBiomeTorches", MySqlDbType.Int32),
            new SqlColumn("happyFunTorchTime", MySqlDbType.Int32),
            new SqlColumn("unlockedBiomeTorches", MySqlDbType.Int32),
            new SqlColumn("currentLoadoutIndex", MySqlDbType.Int32),
            new SqlColumn("ateArtisanBread", MySqlDbType.Int32),
            new SqlColumn("usedAegisCrystal", MySqlDbType.Int32),
            new SqlColumn("usedAegisFruit", MySqlDbType.Int32),
            new SqlColumn("usedArcaneCrystal", MySqlDbType.Int32),
            new SqlColumn("usedGalaxyPearl", MySqlDbType.Int32),
            new SqlColumn("usedGummyWorm", MySqlDbType.Int32),
            new SqlColumn("usedAmbrosia", MySqlDbType.Int32),
            new SqlColumn("unlockedSuperCart", MySqlDbType.Int32),
            new SqlColumn("enabledSuperCart", MySqlDbType.Int32)
            ));
    }
    #endregion

    #region 创建数据方法
    public bool AddData(MyPlayerData data)
    {
        var buff = JsonConvert.SerializeObject(data.Buff);
        return TShock.DB.Query("INSERT INTO " + RolePlayer + " (Name, Role, Cooldown, CoolTime, Buff) VALUES (@0, @1, @2, @3, @4)",
        data.Name, data.Role,data.Cooldown ? 1 : 0,data.CoolTime, buff) != 0;
    }
    #endregion

    #region 创建玩家的角色数据
    public bool AddRoleData(TSPlayer plr, string role)
    {
        if (!plr.IsLoggedIn) return false;
        var tplr = plr.TPlayer;
        var pd = TShock.CharacterDB.GetPlayerData(plr, plr.Account.ID);
        pd.CopyCharacter(plr);
        var AccAndSlot = plr.Account.ID.ToString() + "-" + role;

        return TShock.DB.Query("INSERT INTO " + RoleData + " (AccAndSlot, Role, Account, Name, Health, MaxHealth, Mana, MaxMana, Inventory, extraSlot, spawnX, spawnY, skinVariant, hair, hairDye, hairColor, pantsColor, shirtColor, underShirtColor, shoeColor, hideVisuals, skinColor, eyeColor, questsCompleted, usingBiomeTorches, happyFunTorchTime, unlockedBiomeTorches, currentLoadoutIndex,ateArtisanBread, usedAegisCrystal, usedAegisFruit, usedArcaneCrystal, usedGalaxyPearl, usedGummyWorm, usedAmbrosia, unlockedSuperCart, enabledSuperCart) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12, @13, @14, @15, @16, @17, @18, @19, @20, @21, @22, @23, @24, @25, @26, @27, @28, @29, @30, @31, @32, @33, @34, @35, @36)",
            AccAndSlot, role, plr.Account.ID, plr.Account.Name,
            tplr.statLife, tplr.statLifeMax, tplr.statMana, tplr.statManaMax,
            string.Join("~", pd.inventory), tplr.extraAccessory ? 1 : 0,
            tplr.SpawnX, tplr.SpawnY, tplr.skinVariant, tplr.hair, tplr.hairDye,
            TShock.Utils.EncodeColor(new Color?(tplr.hairColor)), TShock.Utils.EncodeColor(new Color?(tplr.pantsColor)),
            TShock.Utils.EncodeColor(new Color?(tplr.shirtColor)), TShock.Utils.EncodeColor(new Color?(tplr.underShirtColor)),
            TShock.Utils.EncodeColor(new Color?(tplr.shoeColor)), TShock.Utils.EncodeBoolArray(tplr.hideVisibleAccessory),
            TShock.Utils.EncodeColor(new Color?(tplr.skinColor)), TShock.Utils.EncodeColor(new Color?(tplr.eyeColor)),
            tplr.anglerQuestsFinished, tplr.UsingBiomeTorches ? 1 : 0, tplr.happyFunTorchTime ? 1 : 0,
            tplr.unlockedBiomeTorches ? 1 : 0, tplr.CurrentLoadoutIndex, tplr.ateArtisanBread ? 1 : 0,
            tplr.usedAegisCrystal ? 1 : 0, tplr.usedAegisFruit ? 1 : 0, tplr.usedArcaneCrystal ? 1 : 0,
            tplr.usedGalaxyPearl ? 1 : 0, tplr.usedGummyWorm ? 1 : 0, tplr.usedAmbrosia ? 1 : 0,
            tplr.unlockedSuperCart ? 1 : 0, tplr.enabledSuperCart ? 1 : 0) != 0;
    }
    #endregion

    #region 更新数据方法
    public bool UpdateData(MyPlayerData data)
    {
        var buff = JsonConvert.SerializeObject(data.Buff);
        return TShock.DB.Query("UPDATE " + RolePlayer + " SET Role = @0, Cooldown = @1,CoolTime = @2, Buff = @3 WHERE Name = @4",
            data.Role, data.Cooldown ? 1 : 0, data.CoolTime, buff, data.Name) != 0;
    }
    #endregion

    #region 更新玩家的角色数据
    public bool UpdateRoleDB(TSPlayer plr, string role)
    {
        if (!plr.IsLoggedIn) return false;
        var tplr = plr.TPlayer;
        var pd = TShock.CharacterDB.GetPlayerData(plr, plr.Account.ID);
        pd.CopyCharacter(plr);
        var AccAndSlot = plr.Account.ID.ToString() + "-" + role;

        return TShock.DB.Query("UPDATE " + RoleData + " SET Name = @0, Health = @1, MaxHealth = @2, Mana = @3, MaxMana = @4, Inventory = @5, Account = @6, spawnX = @7, spawnY = @8, hair = @9, hairDye = @10, hairColor = @11, pantsColor = @12, shirtColor = @13, underShirtColor = @14, shoeColor = @15, hideVisuals = @16, skinColor = @17, eyeColor = @18, questsCompleted = @19, skinVariant = @20, extraSlot = @21, usingBiomeTorches = @22, happyFunTorchTime = @23, unlockedBiomeTorches = @24, currentLoadoutIndex = @25, ateArtisanBread = @26, usedAegisCrystal = @27, usedAegisFruit = @28, usedArcaneCrystal = @29, usedGalaxyPearl = @30, usedGummyWorm = @31, usedAmbrosia = @32, unlockedSuperCart = @33, enabledSuperCart = @34, Role = @35 WHERE AccAndSlot = @36",
            plr.Account.Name, tplr.statLife, tplr.statLifeMax, tplr.statMana, tplr.statManaMax,
            string.Join("~", pd.inventory), plr.Account.ID, tplr.SpawnX, tplr.SpawnY, tplr.hair, tplr.hairDye,
            TShock.Utils.EncodeColor(new Color?(tplr.hairColor)), TShock.Utils.EncodeColor(new Color?(tplr.pantsColor)),
            TShock.Utils.EncodeColor(new Color?(tplr.shirtColor)), TShock.Utils.EncodeColor(new Color?(tplr.underShirtColor)),
            TShock.Utils.EncodeColor(new Color?(tplr.shoeColor)), TShock.Utils.EncodeBoolArray(tplr.hideVisibleAccessory),
            TShock.Utils.EncodeColor(new Color?(tplr.skinColor)), TShock.Utils.EncodeColor(new Color?(tplr.eyeColor)),
            tplr.anglerQuestsFinished, tplr.skinVariant, tplr.extraAccessory ? 1 : 0, tplr.UsingBiomeTorches ? 1 : 0, tplr.happyFunTorchTime ? 1 : 0,
            tplr.unlockedBiomeTorches ? 1 : 0, tplr.CurrentLoadoutIndex, tplr.ateArtisanBread ? 1 : 0, tplr.usedAegisCrystal ? 1 : 0,
            tplr.usedAegisFruit ? 1 : 0, tplr.usedArcaneCrystal ? 1 : 0, tplr.usedGalaxyPearl ? 1 : 0, tplr.usedGummyWorm ? 1 : 0,
            tplr.usedAmbrosia ? 1 : 0, tplr.unlockedSuperCart ? 1 : 0, tplr.enabledSuperCart ? 1 : 0, role, AccAndSlot) != 0;
    }
    #endregion

    #region 获取玩家数据
    public MyPlayerData? GetData(string name)
    {
        using var reader = TShock.DB.QueryReader("SELECT * FROM " + RolePlayer + " WHERE Name = @0", name);

        if (reader.Read())
        {
            return new MyPlayerData(
                name: reader.Get<string>("Name"),
                role: reader.Get<string>("Role"),
                cooldown: reader.Get<bool>("Cooldown"),
                coolTime: reader.Get<DateTime>("CoolTime"),
                buff: JsonConvert.DeserializeObject<Dictionary<int, int>>(reader.Get<string>("Buff"))!
            );
        }

        return null;
    }
    #endregion

    #region 获取所有玩家数据
    public List<MyPlayerData> GetAllData()
    {
        var data = new List<MyPlayerData>();
        using (var reader = TShock.DB.QueryReader("SELECT * FROM " + RolePlayer))
        {
            while (reader.Read())
            {
                data.Add(new MyPlayerData(
                    name: reader.Get<string>("Name"),
                    role: reader.Get<string>("Role"),
                    cooldown: reader.Get<bool>("Cooldown"),
                    coolTime: reader.Get<DateTime>("CoolTime"),
                    buff: JsonConvert.DeserializeObject<Dictionary<int, int>>(reader.Get<string>("Buff"))!
                ));
            }
        }

        return data;
    }
    #endregion

    #region 获取玩家的角色数据
    public MyRoleData? GetRoleData(TSPlayer plr, string role)
    {
        var accAndSlot = $"{plr.Account.ID}-{role}";
        using var reader = TShock.DB.QueryReader("SELECT * FROM " + RoleData + " WHERE AccAndSlot = @0", accAndSlot);

        if (reader.Read())
        {
            return new MyRoleData(
                accAndSlot: reader.Get<string>("AccAndSlot"),
                role: reader.Get<string>("Role"),
                account: reader.Get<int>("Account"),
                name: reader.Get<string>("Name"),
                health: reader.Get<int>("Health"),
                maxHealth: reader.Get<int>("MaxHealth"),
                mana: reader.Get<int>("Mana"),
                maxMana: reader.Get<int>("MaxMana"),
                inventoryString: reader.Get<string>("Inventory"),
                extraslot: reader.Get<bool>("extraSlot"),
                spawnx: reader.Get<int>("spawnX"),
                spawny: reader.Get<int>("spawnY"),
                skinvariant: reader.Get<int>("skinVariant"),
                hairs: reader.Get<int>("hair"),
                hairdye: reader.Get<byte>("hairDye"),
                haircolor: reader.Get<long>("hairColor"),
                pantscolor: reader.Get<long>("pantsColor"),
                shirtcolor: reader.Get<long>("shirtColor"),
                undershirtcolor: reader.Get<long>("underShirtColor"),
                shoecolor: reader.Get<long>("shoeColor"),
                hidevisuals: TShock.Utils.DecodeBoolArray(reader.Get<int>("hideVisuals")),
                skincolor: reader.Get<long>("skinColor"),
                eyecolor: reader.Get<long>("eyeColor"),
                questscompleted: reader.Get<int>("questsCompleted"),
                usingbiometorches: reader.Get<bool>("usingBiomeTorches"),
                happyfuntorchtime: reader.Get<bool>("happyFunTorchTime"),
                unlockedbiometorches: reader.Get<bool>("unlockedBiomeTorches"),
                currentloadoutindex: reader.Get<int>("currentLoadoutIndex"),
                ateartisanbread: reader.Get<bool>("ateArtisanBread"),
                usedaegiscrystal: reader.Get<bool>("usedAegisCrystal"),
                usedaegisfruit: reader.Get<bool>("usedAegisFruit"),
                usedarcanecrystal: reader.Get<bool>("usedArcaneCrystal"),
                usedgalaxypearl: reader.Get<bool>("usedGalaxyPearl"),
                usedgummyworm: reader.Get<bool>("usedGummyWorm"),
                usedambrosia: reader.Get<bool>("usedAmbrosia"),
                unlockedsupercart: reader.Get<bool>("unlockedSuperCart"),
                enabledsupercart: reader.Get<bool>("enabledSuperCart")
            );
        }

        return null;
    }
    #endregion

    #region 删除指定数据方法
    public bool DelPlayer(string name)
    {
        var tb1 = TShock.DB.Query("DELETE FROM " + RolePlayer + " WHERE Name = @0", name);
        var tb2 = TShock.DB.Query("DELETE FROM " + RoleData + " WHERE Name = @0", name);
        return tb1 != 0 || tb2 != 0;
    }

    public bool DelRole(string role)
    {
        var tb1 = TShock.DB.Query("DELETE FROM " + RolePlayer + " WHERE Role = @0", role);
        var tb2 = TShock.DB.Query("DELETE FROM " + RoleData + " WHERE Role = @0", role);
        return tb1 != 0 || tb2 != 0;
    }
    #endregion

    #region 清理所有数据方法
    public bool ClearData()
    {
        var tb1 = TShock.DB.Query("DELETE FROM " + RolePlayer);
        var tb2 = TShock.DB.Query("DELETE FROM " + RoleData);
        return tb1 != 0 || tb2 != 0;
    }
    #endregion


}