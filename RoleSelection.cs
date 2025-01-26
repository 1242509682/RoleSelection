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
        GetDataHandlers.PlayerUpdate.Register(this.OnPlayerUpdate);
        GetDataHandlers.PlayerSpawn.Register(this.OnPlayerSpawn);
        ServerApi.Hooks.NetGreetPlayer.Register(this, this.OnGreetPlayer);
        TShockAPI.Commands.ChatCommands.Add(new Command("role.use", Commands.RoleCMD, "role", "class", "rl"));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            GeneralHooks.ReloadEvent -= ReloadConfig;
            GetDataHandlers.PlayerUpdate.UnRegister(this.OnPlayerUpdate);
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
                Buff = new Dictionary<int, int>(),
                Cooldown = false,
                CoolTime = DateTime.UtcNow
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

        if (!Config.UseDBSave)
        {
            data.Role = CData.Role;
            DB.UpdateData(data);
            ClearAll(plr); //清除所有
            SetAll(plr, CData); //设置所有
            SetBuff(plr); //设置玩家BUFF
            plr.SendMessage($"已清空背包使用角色 [c/F86570:{CData.Role}]", 170, 170, 170);
        }
        else
        {
            var data2 = DB.GetRoleData(plr, data.Role);
            if (data2 == null)
            {
                ClearAll(plr); //清除所有
                SetAll(plr, CData); //设置配置里的物品
                DB.AddRoleData(plr, CData.Role);
                SetBuff(plr); //设置玩家BUFF
                plr.SendMessage($"已添加角色 [c/F86570:{data.Role}]", 170, 170, 170);
            }
            else if (!data2.Role.Contains(data.Role))
            {
                ClearAll(plr); //清除所有
                SetAll(plr, CData); //设置配置里的物品
                DB.AddRoleData(plr, CData.Role);
                SetBuff(plr); //设置玩家BUFF
                plr.SendMessage($"已添加角色 [c/F86570:{data.Role}]", 170, 170, 170);
            }
            else if (data.Role == data2.Role)
            {
                DB.UpdateRoleDB(plr, data2.Role);
                plr.SendMessage($"已保存角色 [c/5B9DE1:{data2.Role}]", 170, 170, 170);
                data.Role = CData.Role;
                data.Cooldown = true;
                DB.UpdateData(data);
            }
        }
    }
    #endregion

    #region 延迟切换职业方法
    private void OnPlayerUpdate(object? sender, GetDataHandlers.PlayerUpdateEventArgs e)
    {
        var plr = e.Player;
        var data = DB.GetData(plr.Name);
        if (plr == null || !plr.Active || !plr.IsLoggedIn ||
           !plr.HasPermission("role.use") || data == null || !Config.Enabled) return;
        var data2 = DB.GetRoleData(plr, data.Role);
        if (data2 == null) return;

        if (data.Cooldown && (DateTime.UtcNow - data.CoolTime).TotalMilliseconds >= Config.UpdateInterval)
        {
            ClearAll(plr); //清除所有
            SetAll(plr, data2); //设置数据库里的物品
            DB.UpdateRoleDB(plr, data2.Role);
            plr.SendMessage($"已更换角色 [c/5BE1DA:{data2.Role}]", 170, 170, 170);
            data.Cooldown = false;
            data.CoolTime = DateTime.UtcNow;
            DB.UpdateData(data);
            SetBuff(plr); //设置玩家BUFF
        }
    }
    #endregion

    #region 设置玩家所有状态方法
    internal static void SetAll(TSPlayer plr, object data)
    {
        var tplr = plr.TPlayer;
        if (tplr == null) return;

        // 设置玩家生命上限和魔力上限
        var Health = 0;
        var maxHealth = 0;
        var Mana = 0;
        var maxMana = 0;
        var extraSlot = false;
        var spawnX = 0;
        var spawnY = 0;
        var skinVariant = 0;
        var hair = 0;
        byte hairDye = 0;
        var hairColor = new Color(255, 255, 255);
        var pantsColor = new Color(255, 255, 255);
        var shirtColor = new Color(255, 255, 255);
        var shoeColor = new Color(255, 255, 255);
        var underShirtColor = new Color(255, 255, 255);
        var hideVisuals = new bool[] { };
        var skinColor = new Color(255, 255, 255);
        var eyeColor = new Color(255, 255, 255);
        var questsCompleted = 0;
        var usingBiomeTorches = false;
        var happyFunTorchTime = false;
        var unlockedBiomeTorches = false;
        var currentLoadoutIndex = 0;
        var ateArtisanBread = false;
        var usedAegisCrystal = false;
        var usedAegisFruit = false;
        var usedArcaneCrystal = false;
        var usedGalaxyPearl = false;
        var usedGummyWorm = false;
        var usedAmbrosia = false;
        var unlockedSuperCart = false;
        var enabledSuperCart = false;

        // 初始化物品栏和盔甲栏
        var netInv = Array.Empty<NetItem>();
        var netArmor = Array.Empty<NetItem>();
        var DyeIndex = Array.Empty<NetItem>();
        var MiscEquipIndex = Array.Empty<NetItem>();
        var MiscDyeIndex = Array.Empty<NetItem>();
        var PiggySlots = Array.Empty<NetItem>();
        var SafeSlots = Array.Empty<NetItem>();
        var TrashIndex = Array.Empty<NetItem>();
        var ForgeIndex = Array.Empty<NetItem>();
        var VoidIndex = Array.Empty<NetItem>();
        var loadout1Armor = Array.Empty<NetItem>();
        var Loadout1Dye = Array.Empty<NetItem>();
        var loadout2Armor = Array.Empty<NetItem>();
        var loadout2Dye = Array.Empty<NetItem>();
        var loadout3Armor = Array.Empty<NetItem>();
        var loadout3Dye = Array.Empty<NetItem>();

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
            Health = db.Health;
            Mana = db.Mana;
            maxHealth = db.MaxHealth;
            maxMana = db.MaxMana;
            extraSlot = db.extraSlot;
            spawnX = db.spawnX;
            spawnY = db.spawnY;
            skinVariant = db.skinVariant;
            hair = db.hair;
            hairDye = db.hairDye;
            netInv = Utils.StringToItem(db.Inventory);
            netArmor = Utils.StringToItem2(db.Inventory, NetItem.ArmorIndex.Item1, NetItem.ArmorSlots);

            DyeIndex = Utils.StringToItem2(db.Inventory, NetItem.DyeIndex.Item1, NetItem.DyeIndex.Item2);
            MiscEquipIndex = Utils.StringToItem2(db.Inventory, NetItem.MiscEquipIndex.Item1, NetItem.MiscEquipIndex.Item2);
            MiscDyeIndex = Utils.StringToItem2(db.Inventory, NetItem.MiscDyeIndex.Item1, NetItem.MiscDyeIndex.Item2);
            PiggySlots = Utils.StringToItem2(db.Inventory, NetItem.PiggyIndex.Item1, NetItem.PiggyIndex.Item2);
            SafeSlots = Utils.StringToItem2(db.Inventory, NetItem.SafeIndex.Item1, NetItem.SafeIndex.Item2);
            TrashIndex = Utils.StringToItem2(db.Inventory, NetItem.TrashIndex.Item1, NetItem.TrashIndex.Item2);
            ForgeIndex = Utils.StringToItem2(db.Inventory, NetItem.ForgeIndex.Item1, NetItem.ForgeIndex.Item2);
            VoidIndex = Utils.StringToItem2(db.Inventory, NetItem.VoidIndex.Item1, NetItem.VoidIndex.Item2);
            loadout1Armor = Utils.StringToItem2(db.Inventory, NetItem.Loadout1Armor.Item1, NetItem.Loadout1Armor.Item2);
            Loadout1Dye = Utils.StringToItem2(db.Inventory, NetItem.Loadout1Dye.Item1, NetItem.Loadout1Dye.Item2);
            loadout2Armor = Utils.StringToItem2(db.Inventory, NetItem.Loadout2Armor.Item1, NetItem.Loadout2Armor.Item2);
            loadout2Dye = Utils.StringToItem2(db.Inventory, NetItem.Loadout2Dye.Item1, NetItem.Loadout2Dye.Item2);
            loadout3Armor = Utils.StringToItem2(db.Inventory, NetItem.Loadout3Armor.Item1, NetItem.Loadout3Armor.Item2);
            loadout3Dye = Utils.StringToItem2(db.Inventory, NetItem.Loadout3Dye.Item1, NetItem.Loadout3Dye.Item2);

            hairColor = Utils.DecodeColor(db.hairColor);
            pantsColor = Utils.DecodeColor(db.pantsColor);
            shirtColor = Utils.DecodeColor(db.shirtColor);
            eyeColor = Utils.DecodeColor(db.eyeColor);
            skinColor = Utils.DecodeColor(db.skinColor);
            underShirtColor = Utils.DecodeColor(db.underShirtColor);
            shoeColor = Utils.DecodeColor(db.shoeColor);
            hideVisuals = db.hideVisuals;
            questsCompleted = db.questsCompleted;
            usingBiomeTorches = db.usingBiomeTorches;
            happyFunTorchTime = db.happyFunTorchTime;
            unlockedBiomeTorches = db.unlockedBiomeTorches;
            currentLoadoutIndex = db.currentLoadoutIndex;
            ateArtisanBread = db.ateArtisanBread;
            usedAegisCrystal = db.usedAegisCrystal;
            usedAegisFruit = db.usedAegisFruit;
            usedArcaneCrystal = db.usedArcaneCrystal;
            usedGalaxyPearl = db.usedGalaxyPearl;
            usedGummyWorm = db.usedGummyWorm;
            usedAmbrosia = db.usedAmbrosia;
            unlockedSuperCart = db.unlockedSuperCart;
            enabledSuperCart = db.enabledSuperCart;
        }

        // 设置玩家生命上限
        tplr.statLife = tplr.statLifeMax = maxHealth;
        plr.SendData(PacketTypes.PlayerHp, "", plr.Index);

        // 设置玩家魔力上限
        tplr.statMana = tplr.statManaMax = maxMana;
        plr.SendData(PacketTypes.PlayerMana, "", plr.Index);

        tplr.SpawnX = spawnX;
        tplr.SpawnY = spawnY;
        tplr.skinVariant = skinVariant;
        tplr.hair = hair;
        tplr.hairDye = hairDye;
        tplr.hairColor = hairColor;
        tplr.pantsColor = pantsColor;
        tplr.shirtColor = shirtColor;
        tplr.underShirtColor = underShirtColor;
        tplr.shoeColor = shoeColor;
        tplr.hideVisibleAccessory = hideVisuals;
        tplr.skinColor = skinColor;
        tplr.eyeColor = eyeColor;
        tplr.anglerQuestsFinished = questsCompleted;
        tplr.extraAccessory = extraSlot;
        tplr.UsingBiomeTorches = usingBiomeTorches;
        tplr.happyFunTorchTime = happyFunTorchTime;
        tplr.unlockedBiomeTorches = unlockedBiomeTorches;
        tplr.CurrentLoadoutIndex = currentLoadoutIndex;
        tplr.ateArtisanBread = ateArtisanBread;
        tplr.usedAegisCrystal = usedAegisCrystal;
        tplr.usedAegisFruit = usedAegisFruit;
        tplr.usedArcaneCrystal = usedArcaneCrystal;
        tplr.usedGalaxyPearl = usedGalaxyPearl;
        tplr.usedGummyWorm = usedGummyWorm;
        tplr.usedAmbrosia = usedAmbrosia;
        tplr.unlockedSuperCart = unlockedSuperCart;
        tplr.enabledSuperCart = enabledSuperCart;
        plr.SendData(PacketTypes.PlayerInfo, "", plr.Index, 0f, 0f, 0f, 0);

        SetItem(plr, tplr.inventory, netInv, NetItem.InventoryIndex.Item1, NetItem.InventorySlots);
        SetItem(plr, tplr.armor, netArmor, NetItem.ArmorIndex.Item1, NetItem.ArmorSlots);
        SetItem(plr, tplr.dye, DyeIndex, NetItem.DyeIndex.Item1, NetItem.DyeSlots);
        SetItem(plr, tplr.miscEquips, MiscEquipIndex, NetItem.MiscEquipIndex.Item1, NetItem.MiscEquipSlots);
        SetItem(plr, tplr.miscDyes, MiscDyeIndex, NetItem.MiscDyeIndex.Item1, NetItem.MiscDyeSlots);
        SetItem(plr, tplr.bank.item, PiggySlots, NetItem.PiggyIndex.Item1, NetItem.PiggySlots);
        SetItem(plr, tplr.bank2.item, SafeSlots, NetItem.SafeIndex.Item1, NetItem.SafeSlots);
        SetItem(plr, tplr.bank3.item, ForgeIndex, NetItem.ForgeIndex.Item1, NetItem.ForgeSlots);
        SetItem(plr, tplr.bank4.item, VoidIndex, NetItem.VoidIndex.Item1, NetItem.VoidSlots);
        SetItem(plr, tplr.Loadouts[0].Armor, loadout1Armor, NetItem.Loadout1Armor.Item1, NetItem.LoadoutArmorSlots);
        SetItem(plr, tplr.Loadouts[0].Dye, Loadout1Dye, NetItem.Loadout1Dye.Item1, NetItem.LoadoutDyeSlots);
        SetItem(plr, tplr.Loadouts[1].Armor, loadout2Armor, NetItem.Loadout2Armor.Item1, NetItem.LoadoutArmorSlots);
        SetItem(plr, tplr.Loadouts[1].Dye, loadout2Dye, NetItem.Loadout2Dye.Item1, NetItem.LoadoutDyeSlots);
        SetItem(plr, tplr.Loadouts[2].Armor, loadout3Armor, NetItem.Loadout3Armor.Item1, NetItem.LoadoutArmorSlots);
        SetItem(plr, tplr.Loadouts[2].Dye, loadout3Dye, NetItem.Loadout3Dye.Item1, NetItem.LoadoutDyeSlots);

    }
    #endregion

    #region 设置物品方法
    private static void SetItem(TSPlayer plr, Item[] plrItem, NetItem[] items, int start, int MaxSlot)
    {
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