using Microsoft.Xna.Framework;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Terraria.Net.Sockets;
using TShockAPI;
using TShockAPI.DB;
using static MonoMod.InlineRT.MonoModRule;

namespace RoleSelection;

public class PlayerRole
{
    public string Name { get; set; } = ""; // 玩家名字
    public string Role { get; set; } = "无"; // 角色名称
    public bool Lock = false; // 是否锁定
    public Dictionary<int, int> Buff { get; set; } = new Dictionary<int, int>();
    public PlayerRole(string name = "", string role = "无",bool Lock = false, Dictionary<int, int> buff = null!)
    {
        this.Name = name ?? "";
        this.Role = role ?? "无";
        this.Lock = Lock;
        this.Buff = buff ?? new Dictionary<int, int>();
    }
}

public class Database
{
    #region 数据库表结构（使用Tshock自带的数据库作为存储）
    private readonly string RolePlayer;
    private readonly string RoleData;
    public List<RoleData> rolePlayers = new();
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
            new SqlColumn("Role", MySqlDbType.Text),
            new SqlColumn("Lock", MySqlDbType.Int32) { DefaultValue = "0" }, // bool值列
            new SqlColumn("Buff", MySqlDbType.Text)
        ));

        RoleData = "RoleData";
        sql.EnsureTableStructure(new SqlTable(RoleData,
            new SqlColumn("Role", MySqlDbType.Text),
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

        ReadPlayerDatas();
    }
    #endregion

    #region 创建与更新玩家、角色数据
    public bool AddPlayer(PlayerRole data)
    {
        var buff = JsonConvert.SerializeObject(data.Buff);
        return TShock.DB.Query("INSERT INTO " + RolePlayer + " (Name, Role, Lock, Buff) VALUES (@0, @1, @2, @3)",
        data.Name, data.Role, data.Lock ? 1 : 0, buff) != 0;
    }

    public bool UpdatePlayer(PlayerRole data)
    {
        var buff = JsonConvert.SerializeObject(data.Buff);
        return TShock.DB.Query("UPDATE " + RolePlayer + " SET Role = @0, Lock = @1, Buff = @2 WHERE Name = @3",
            data.Role, data.Lock ? 1 : 0, buff, data.Name) != 0;
    }

    public bool RoleLock(string name, bool Locks)
    {
       return TShock.DB.Query("UPDATE RolePlayer SET Lock = @0 WHERE Name = @1", Locks ? 1 : 0, name) != 0;
    }
    #endregion

    #region 获取玩家数据
    public PlayerRole? GetData(string name)
    {
        using var reader = TShock.DB.QueryReader("SELECT * FROM " + RolePlayer + " WHERE Name = @0", name);

        if (reader.Read())
        {
            return new PlayerRole(
                name: reader.Get<string>("Name"),
                role: reader.Get<string>("Role"),
                Lock: reader.Get<bool>("Lock"),
                buff: JsonConvert.DeserializeObject<Dictionary<int, int>>(reader.Get<string>("Buff"))!
            );
        }

        return null;
    }

    public List<PlayerRole> GetAllData()
    {
        var data = new List<PlayerRole>();
        using (var reader = TShock.DB.QueryReader("SELECT * FROM " + RolePlayer))
        {
            while (reader.Read())
            {
                data.Add(new PlayerRole(
                    name: reader.Get<string>("Name"),
                    role: reader.Get<string>("Role"),
                    Lock: reader.Get<bool>("Lock"),
                    buff: JsonConvert.DeserializeObject<Dictionary<int, int>>(reader.Get<string>("Buff"))!
                ));
            }
        }

        return data;
    }
    #endregion

    #region 删除指定数据方法
    public bool DelPlayer(string name)
    {
        var acc = TShock.UserAccounts.GetUserAccountByName(name);
        var tb1 = TShock.DB.Query("DELETE FROM " + RolePlayer + " WHERE Name = @0", name);
        var tb2 = TShock.DB.Query("DELETE FROM " + RoleData + " WHERE Account = @0", acc);
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

    /* ——————————由少司命贡献———————————— */

    #region 读取玩家数据
    public void ReadPlayerDatas()
    {
        try
        {
            using var reader = TShock.DB.QueryReader("SELECT * FROM RoleData");
            while (reader.Read())
            {
                RoleData pd = new()
                {
                    exists = true,
                    Account = reader.Get<int>("Account"),
                    Role = reader.Get<string>("Role"),
                    health = reader.Get<int>("Health"),
                    maxHealth = reader.Get<int>("MaxHealth"),
                    mana = reader.Get<int>("Mana"),
                    maxMana = reader.Get<int>("MaxMana")
                };
                List<NetItem> inventory = reader.Get<string>("Inventory").Split('~').Select(NetItem.Parse).ToList();
                if (inventory.Count < NetItem.MaxInventory)
                {
                    //TODO: unhardcode this - stop using magic numbers and use NetItem numbers
                    //Set new armour slots empty
                    inventory.InsertRange(67, new NetItem[2]);
                    //Set new vanity slots empty
                    inventory.InsertRange(77, new NetItem[2]);
                    //Set new dye slots empty
                    inventory.InsertRange(87, new NetItem[2]);
                    //Set the rest of the new slots empty
                    inventory.AddRange(new NetItem[NetItem.MaxInventory - inventory.Count]);
                }
                pd.inventory = inventory.ToArray();
                pd.extraSlot = reader.Get<int>("extraSlot");
                pd.spawnX = reader.Get<int>("spawnX");
                pd.spawnY = reader.Get<int>("spawnY");
                pd.skinVariant = reader.Get<int?>("skinVariant");
                pd.hair = reader.Get<int?>("hair");
                pd.hairDye = (byte)reader.Get<int>("hairDye");
                pd.hairColor = TShock.Utils.DecodeColor(reader.Get<int?>("hairColor"));
                pd.pantsColor = TShock.Utils.DecodeColor(reader.Get<int?>("pantsColor"));
                pd.shirtColor = TShock.Utils.DecodeColor(reader.Get<int?>("shirtColor"));
                pd.underShirtColor = TShock.Utils.DecodeColor(reader.Get<int?>("underShirtColor"));
                pd.shoeColor = TShock.Utils.DecodeColor(reader.Get<int?>("shoeColor"));
                pd.hideVisuals = TShock.Utils.DecodeBoolArray(reader.Get<int?>("hideVisuals"));
                pd.skinColor = TShock.Utils.DecodeColor(reader.Get<int?>("skinColor"));
                pd.eyeColor = TShock.Utils.DecodeColor(reader.Get<int?>("eyeColor"));
                pd.questsCompleted = reader.Get<int>("questsCompleted");
                pd.usingBiomeTorches = reader.Get<int>("usingBiomeTorches");
                pd.happyFunTorchTime = reader.Get<int>("happyFunTorchTime");
                pd.unlockedBiomeTorches = reader.Get<int>("unlockedBiomeTorches");
                pd.currentLoadoutIndex = reader.Get<int>("currentLoadoutIndex");
                pd.ateArtisanBread = reader.Get<int>("ateArtisanBread");
                pd.usedAegisCrystal = reader.Get<int>("usedAegisCrystal");
                pd.usedAegisFruit = reader.Get<int>("usedAegisFruit");
                pd.usedArcaneCrystal = reader.Get<int>("usedArcaneCrystal");
                pd.usedGalaxyPearl = reader.Get<int>("usedGalaxyPearl");
                pd.usedGummyWorm = reader.Get<int>("usedGummyWorm");
                pd.usedAmbrosia = reader.Get<int>("usedAmbrosia");
                pd.unlockedSuperCart = reader.Get<int>("unlockedSuperCart");
                pd.enabledSuperCart = reader.Get<int>("enabledSuperCart");
                rolePlayers.Add(pd);
            }
        }
        catch (Exception ex)
        {
            TShock.Log.Error(ex.ToString());
        }
    }
    #endregion

    #region 创建与更新玩家数据
    public bool SetAndUpdate(TSPlayer plr, RoleData data, string role)
    {
        if (!plr.IsLoggedIn) return false;
        var pd = data;
        var old = GetData2(plr, role);
        if (old == null)
        {
            try
            {
                TShock.DB.Query(
                    "INSERT INTO RoleData (Account, Name, Health, MaxHealth, Mana, MaxMana, Inventory, extraSlot, spawnX, spawnY, skinVariant, hair, hairDye, hairColor, pantsColor, shirtColor, underShirtColor, shoeColor, hideVisuals, skinColor, eyeColor, questsCompleted, usingBiomeTorches, happyFunTorchTime, unlockedBiomeTorches, currentLoadoutIndex, ateArtisanBread, usedAegisCrystal, usedAegisFruit, usedArcaneCrystal, usedGalaxyPearl, usedGummyWorm, usedAmbrosia, unlockedSuperCart, enabledSuperCart, Role) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12, @13, @14, @15, @16, @17, @18, @19, @20, @21, @22, @23, @24, @25, @26, @27, @28, @29, @30, @31, @32, @33, @34, @35);",
                    plr.Account.ID,
                    plr.Name,
                    pd.health,
                    pd.maxHealth,
                    pd.mana,
                    pd.maxMana,
                    String.Join("~", pd.inventory),
                    pd.extraSlot,
                    pd.spawnX,
                    pd.spawnX,
                    pd.skinVariant,
                    pd.hair,
                    pd.hairDye,
                    TShock.Utils.EncodeColor(plr.TPlayer.hairColor),
                    TShock.Utils.EncodeColor(plr.TPlayer.pantsColor),
                    TShock.Utils.EncodeColor(plr.TPlayer.shirtColor),
                    TShock.Utils.EncodeColor(plr.TPlayer.underShirtColor),
                    TShock.Utils.EncodeColor(plr.TPlayer.shoeColor),
                    TShock.Utils.EncodeBoolArray(pd.hideVisuals),
                    TShock.Utils.EncodeColor(plr.TPlayer.skinColor),
                    TShock.Utils.EncodeColor(plr.TPlayer.eyeColor),
                    pd.questsCompleted,
                    pd.usingBiomeTorches,
                    pd.happyFunTorchTime,
                    pd.unlockedBiomeTorches,
                    pd.currentLoadoutIndex,
                    pd.ateArtisanBread,
                    pd.usedAegisCrystal,
                    pd.usedAegisFruit,
                    pd.usedArcaneCrystal,
                    pd.usedGalaxyPearl,
                    pd.usedGummyWorm,
                    pd.usedAmbrosia,
                    pd.unlockedSuperCart,
                    pd.enabledSuperCart,
                    role);
                rolePlayers.Add(pd);
                return true;
            }
            catch (Exception ex)
            {
                TShock.Log.Error(ex.ToString());
            }
        }
        else
        {
            try
            {
                TShock.DB.Query(
                    "UPDATE RoleData SET Health = @0, MaxHealth = @1, Mana = @2, MaxMana = @3, Inventory = @4, spawnX = @6, spawnY = @7, hair = @8, hairDye = @9, hairColor = @10, pantsColor = @11, shirtColor = @12, underShirtColor = @13, shoeColor = @14, hideVisuals = @15, skinColor = @16, eyeColor = @17, questsCompleted = @18, skinVariant = @19, extraSlot = @20, usingBiomeTorches = @21, happyFunTorchTime = @22, unlockedBiomeTorches = @23, currentLoadoutIndex = @24, ateArtisanBread = @25, usedAegisCrystal = @26, usedAegisFruit = @27, usedArcaneCrystal = @28, usedGalaxyPearl = @29, usedGummyWorm = @30, usedAmbrosia = @31, unlockedSuperCart = @32, enabledSuperCart = @33, Name = @34 WHERE Account = @5 AND Role = @35;",
                    pd.health,
                    pd.maxHealth,
                    pd.mana,
                    pd.maxMana,
                    String.Join("~", pd.inventory),
                    plr.Account.ID,
                    pd.spawnX,
                    pd.spawnX,
                    pd.skinVariant,
                    pd.hair,
                    pd.hairDye,
                    TShock.Utils.EncodeColor(plr.TPlayer.hairColor),
                    TShock.Utils.EncodeColor(plr.TPlayer.pantsColor),
                    TShock.Utils.EncodeColor(plr.TPlayer.shirtColor),
                    TShock.Utils.EncodeColor(plr.TPlayer.underShirtColor),
                    TShock.Utils.EncodeColor(plr.TPlayer.shoeColor),
                    TShock.Utils.EncodeBoolArray(pd.hideVisuals),
                    TShock.Utils.EncodeColor(plr.TPlayer.skinColor),
                    TShock.Utils.EncodeColor(plr.TPlayer.eyeColor),
                    pd.questsCompleted,
                    pd.extraSlot ?? 0,
                    pd.usingBiomeTorches,
                    pd.happyFunTorchTime,
                    pd.unlockedBiomeTorches,
                    pd.currentLoadoutIndex,
                    pd.ateArtisanBread,
                    pd.usedAegisCrystal,
                    pd.usedAegisFruit,
                    pd.usedArcaneCrystal,
                    pd.usedGalaxyPearl,
                    pd.usedGummyWorm,
                    pd.usedAmbrosia,
                    pd.unlockedSuperCart,
                    pd.enabledSuperCart,
                    plr.Name,
                    role);
                return true;
            }
            catch (Exception ex)
            {
                TShock.Log.Error(ex.ToString());
            }
        }
        return false;
    }
    #endregion

    #region 获取玩家数据方法
    public RoleData? GetData2(TSPlayer plr, string role)
    {
        return rolePlayers.FirstOrDefault(p => p.Role == role && p.Account == plr.Account.ID);
    }

    public List<RoleData> GetRole(int userid)
    {
        return rolePlayers.FindAll(p => p.Account == userid);
    }
    #endregion

    #region 移除玩家数据方法
    public bool RmPlayer(int userid, string role)
    {
        try
        {
            if (rolePlayers.RemoveAll(p => p.Account == userid && p.Role == role) > 0)
            {
                TShock.DB.Query("DELETE FROM RoleCharacter WHERE Account = @0 AND Role = @1;", userid, role);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            TShock.Log.Error(ex.ToString());
        }

        return false;
    } 
    #endregion
}