using Microsoft.Xna.Framework;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using static RoleSelection.Configuration;

namespace RoleSelection;

[ApiVersion(2, 1)]
public class RoleSelection : TerrariaPlugin
{
    #region 插件信息
    public override string Name => "角色选择系统";
    public override string Author => "SAP 羽学";
    public override Version Version => new Version(1, 0, 2);
    public override string Description => "使用指令选择角色存档";
    #endregion

    #region 全局变量
    internal static Configuration Config = new();
    public static Database DB = new();
    #endregion

    #region 注册与释放
    public RoleSelection(Main game) : base(game) { }

    public override void Initialize()
    {
        LoadConfig();
        GeneralHooks.ReloadEvent += ReloadConfig;
        GetDataHandlers.PlayerSpawn.Register(this.OnPlayerSpawn);
        ServerApi.Hooks.NetGreetPlayer.Register(this, this.OnGreetPlayer);
        TShockAPI.Commands.ChatCommands.Add(new Command("role.use", Commands.RoleCMD, "role", "class", "rl"));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            GeneralHooks.ReloadEvent -= ReloadConfig;
            GetDataHandlers.PlayerSpawn.UnRegister(this.OnPlayerSpawn);
            ServerApi.Hooks.NetGreetPlayer.Deregister(this, this.OnGreetPlayer);
            TShockAPI.Commands.ChatCommands.RemoveAll(x => x.CommandDelegate == Commands.RoleCMD);
        }
        base.Dispose(disposing);
    }
    #endregion

    #region 配置重载读取与写入方法
    private static void ReloadConfig(ReloadEventArgs args = null!)
    {
        LoadConfig();
        args.Player.SendInfoMessage("[角色选择系统]重新加载配置完毕。");
    }

    private static void LoadConfig()
    {
        Config = Configuration.Read();
        Config.Write();
    }
    #endregion

    #region 玩家生成事件（给BUFF用）
    private void OnPlayerSpawn(object? sender, GetDataHandlers.SpawnEventArgs e)
    {
        var plr = e.Player;
        if (plr == null || !plr.Active || !plr.IsLoggedIn || !Config.Enabled || !plr.HasPermission("role.use")) return;

        SetBuff(plr);
    }
    #endregion

    #region 进服更新创建数据方法
    private void OnGreetPlayer(GreetPlayerEventArgs args)
    {
        var plr = TShock.Players[args.Who];
        if (plr == null || !plr.Active || !plr.IsLoggedIn || !plr.HasPermission("role.use") || !Config.Enabled) return;

        var data = DB.GetData(plr.Name); //获取玩家数据方法
        if (data == null) //如果没有获取到的玩家数据
        {
            data = new MyPlayerData()
            {
                Name = plr.Name,
                Role = "萌新",
                Buff = new Dictionary<int, int>()
            };

            DB.AddData(data); //添加新数据

            //设置新玩家职业
            foreach (var role in Config.MyDataList)
            {
                if (data.Role == role.Role)
                {
                    Rank(plr, data, role);
                }
            }
        }
        else
        {
            SetBuff(plr);
        }

        if (Config.JoinClearItem) //进服清理一切
        {
            ClearAll(plr);
        }
    }
    #endregion

    #region 选职业方法
    internal static void Rank(TSPlayer plr, MyPlayerData? data, Configuration.MyData CData)
    {
        if (data == null || CData.Role == "无" || !plr.HasPermission("role.use")) return;

        data.Buff = CData.Buff;
        data.Role = CData.Role;
        DB.UpdateData(data);

        if (!Config.UseDBSave)
        {
            ClearAll(plr); //清除所有
            SetAll(plr, CData); //设置所有
            plr.SendMessage($"已清理背包选择角色 [c/F86570:{CData.Role}]", 240, 250, 150);
        }
        else
        {
            var data2 = DB.GetRoleData(plr, data.Role);
            if (data2 == null || !data2.Role.Contains(data.Role))
            {
                ClearAll(plr); //清除所有
                SetAll(plr, CData); //设置配置里的物品
                DB.AddRoleData(plr, data.Role);
                plr.SendMessage($"已添加角色 [c/F86570:{data.Role}]", 240, 250, 150);
            }
            else
            {
                ClearAll(plr); //清除所有
                SetAll(plr, data2); //设置数据库里的物品
                DB.UpdateRoleDB(plr, data.Role);
                plr.SendMessage($"已选择角色 [c/5B9DE1:{data2.Role}]", 240, 250, 150);
            }
        }

        SetBuff(plr); //设置玩家BUFF
    }
    #endregion

    #region 设置玩家所有状态方法
    internal static void SetAll(TSPlayer plr, object data)
    {
        var tplr = plr.TPlayer;
        if (tplr == null) return;

        // 设置玩家生命上限和魔力上限
        var maxHealth = 0;
        var maxMana = 0;

        // 初始化物品栏和盔甲栏
        var netInv = Array.Empty<NetItem>();
        var netArmor = Array.Empty<NetItem>();
        var loadout1Armor = Array.Empty<NetItem>();
        var loadout2Armor = Array.Empty<NetItem>();
        var loadout3Armor = Array.Empty<NetItem>();

        var hairColor = new Color(255, 255, 255);
        var pantsColor = new Color(255, 255, 255);
        var shirtColor = new Color(255, 255, 255);
        var eyeColor = new Color(255, 255, 255);
        var skinColor = new Color(255, 255, 255);
        var underShirtColor = new Color(255, 255, 255);
        var shoeColor = new Color(255, 255, 255);

        // 根据传入的数据类型设置相应的属性
        if (data is MyData my)
        {
            maxHealth = my.maxHealth;
            maxMana = my.maxMana;
            netInv = my.inventory ?? Array.Empty<NetItem>();
            netArmor = my.armor ?? Array.Empty<NetItem>();
            loadout1Armor = my.loadout1Armor ?? Array.Empty<NetItem>();
            loadout2Armor = my.loadout2Armor ?? Array.Empty<NetItem>();
            loadout3Armor = my.loadout3Armor ?? Array.Empty<NetItem>();
        }
        else if (data is MyRoleData db)
        {
            maxHealth = db.MaxHealth;
            maxMana = db.MaxMana;
            netInv = Utils.StringToItem(db.Inventory);
            netArmor = Utils.StringToItem2(db.Inventory, NetItem.ArmorIndex.Item1, NetItem.ArmorSlots);
            loadout1Armor = Utils.StringToItem2(db.Inventory, NetItem.Loadout1Armor.Item1, NetItem.ArmorSlots);
            loadout2Armor = Utils.StringToItem2(db.Inventory, NetItem.Loadout2Armor.Item1, NetItem.ArmorSlots);
            loadout3Armor = Utils.StringToItem2(db.Inventory, NetItem.Loadout3Armor.Item1, NetItem.ArmorSlots);
            hairColor = Utils.DecodeColor(db.hairColor);
            pantsColor = Utils.DecodeColor(db.pantsColor);
            shirtColor = Utils.DecodeColor(db.shirtColor);
            eyeColor = Utils.DecodeColor(db.eyeColor);
            skinColor = Utils.DecodeColor(db.skinColor);
            underShirtColor = Utils.DecodeColor(db.underShirtColor);
            shoeColor = Utils.DecodeColor(db.shoeColor);
        }

        //设置背包物品
        SetItem(plr, tplr.inventory, netInv, NetItem.InventoryIndex.Item1, NetItem.InventorySlots);
        // 设置当前盔甲栏物品
        SetItem(plr, tplr.armor, netArmor, NetItem.ArmorIndex.Item1, NetItem.ArmorSlots);
        // 设置其他装备页的盔甲与饰品
        SetItem(plr, tplr.Loadouts[0].Armor, loadout1Armor, NetItem.Loadout1Armor.Item1, NetItem.Loadout1Armor.Item2);
        SetItem(plr, tplr.Loadouts[1].Armor, loadout2Armor, NetItem.Loadout2Armor.Item1, NetItem.Loadout2Armor.Item2);
        SetItem(plr, tplr.Loadouts[2].Armor, loadout3Armor, NetItem.Loadout3Armor.Item1, NetItem.Loadout3Armor.Item2);

        // 设置玩家生命上限
        tplr.statLifeMax = maxHealth;
        tplr.statLife = maxHealth;
        plr.SendData(PacketTypes.PlayerHp, "", plr.Index);

        // 设置玩家魔力上限
        tplr.statManaMax = maxMana;
        tplr.statMana = maxMana;
        plr.SendData(PacketTypes.PlayerMana, "", plr.Index);
        DB.UpdateSSC(plr); // 更新SSC数据库
    }
    #endregion

    #region 设置物品方法
    private static void SetItem(TSPlayer plr, Item[] plrItem, NetItem[] items, int start, int MaxSlot)
    {
        var pd = TShock.CharacterDB.GetPlayerData(plr, plr.Account.ID);
        if (items != null && items.Length > 0)
        {
            for (var i = 0; i < Math.Min(items.Length, MaxSlot); i++)
            {
                if (items[i].NetId != 0)
                {
                    plrItem[i] = TShock.Utils.GetItemById(items[i].NetId);
                    plrItem[i].stack = items[i].Stack;
                    plrItem[i].prefix = items[i].PrefixId;
                    plr.SendData(PacketTypes.PlayerSlot, "", plr.Index, i + start);
                    pd.StoreSlot(i + start, plrItem[i].type, plrItem[i].prefix, plrItem[i].stack);
                    pd.CopyCharacter(plr);
                }   
            }
        }
    }
    #endregion

    #region 清除所有
    internal static void ClearAll(TSPlayer plr)
    {
        var tplr = plr.TPlayer;

        for (var i = 0; i < NetItem.MaxInventory; i++)
        {
            // 清理主物品栏、盔甲栏和其他装备栏
            ClearSlots(tplr.inventory);
            ClearSlots(tplr.armor);
            ClearSlots(tplr.dye);
            ClearSlots(tplr.miscEquips);
            ClearSlots(tplr.miscDyes);
            ClearSlots(tplr.bank.item);
            ClearSlots(tplr.bank2.item);
            ClearSlots(tplr.bank3.item);
            ClearSlots(tplr.bank4.item);
            tplr.trashItem.TurnToAir();
            foreach (var loadout in tplr.Loadouts)
            {
                ClearSlots(loadout.Armor);
                ClearSlots(loadout.Dye);
            }

            //清空所有物品
            plr.SendData(PacketTypes.PlayerSlot, "", plr.Index, i);
        }

        //还原玩家生命上限
        tplr.statLife = tplr.statLifeMax = TShock.ServerSideCharacterConfig.Settings.StartingHealth;
        plr.SendData(PacketTypes.PlayerHp, "", plr.Index);

        //还原玩家魔力上限
        tplr.statMana = tplr.statManaMax = TShock.ServerSideCharacterConfig.Settings.StartingMana;
        plr.SendData(PacketTypes.PlayerMana, "", plr.Index);

        //关闭所有被动永久增益
        tplr.unlockedBiomeTorches = tplr.extraAccessory =
        tplr.ateArtisanBread = tplr.usedAegisCrystal =
        tplr.usedAegisFruit = tplr.usedArcaneCrystal =
        tplr.usedGalaxyPearl = tplr.usedGummyWorm =
        tplr.usedAmbrosia = tplr.unlockedSuperCart = false;
        plr.SendData(PacketTypes.PlayerInfo, "", plr.Index, 0f, 0f, 0f, 0);

        // 清空玩家所有BUFF
        Array.Clear(tplr.buffType, 0, tplr.buffType.Length);
        plr.SendData(PacketTypes.PlayerBuff, "", plr.Index);

        DB.UpdateSSC(plr);
    }

    private static void ClearSlots(Item[] items)
    {
        if (items == null) return;
        foreach (var item in items)
        {
            // 如果是钱币 或免清物品表的物品ID则跳过
            if ((!Config.IsACoin && item.IsACoin) ||
                  Config.ExemptList.Contains(item.netID)) continue;
            item.TurnToAir();
        }
    }
    #endregion

    #region 设置BUFF方法
    public static void SetBuff(TSPlayer plr)
    {
        var data = DB.GetData(plr.Name);
        if (data == null) return;

        var timeLimit = (int.MaxValue / 60 / 60) - 1;

        foreach (var buff in data.Buff)
        {
            int id = buff.Key;
            int time = buff.Value;
            if (time < 0 || time > timeLimit) time = timeLimit;
            plr.SetBuff(id, time * 60 * 60);
        }
    }
    #endregion
}